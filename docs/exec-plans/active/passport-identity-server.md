# Plan: Passport Identity Server

**Created:** 2026-06-27
**Status:** 🚀 Active — Phase 1 complete, Phase 2 next
**Project:** Passport
**Driving Agent:** human

## Goal

Build Passport: a self-hosted identity server that serves as the authentication foundation for all Supercluster apps. It implements OAuth 2.0 / OpenID Connect with **passkeys (WebAuthn) as the exclusive first-class authentication method** — no password login. Users register with email (for account recovery) and a passkey. The service is self-hosted first but designed to be deployable by other tech-savvy operators.

## High-Level Architecture

Passport follows Clean Architecture like all Supercluster projects:

```
Passport.Presentation.Http   ← OAuth/OIDC endpoints, login/signup pages (API + SSR or SPA)
Passport.Core.Application    ← commands/queries, ports (interfaces), read models
Passport.Core.Domain         ← User, PasskeyCredential, Client, AuthorizationCode, RefreshToken, Session
Passport.Infrastructure      ← EF Core with Postgres + SQLite adapters, WebAuthn (FIDO2) library
```

Frontend: SolidJS SPA in `apps/passport/` for login, signup, passkey management, and account recovery.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Passkeys only, no passwords | Eliminates credential stuffing, phishing, and password reset flows entirely |
| Email for recovery only | Minimal barrier to signup — just email and a passkey tap; email serves as the recovery channel |
| Hybrid token model (JWT + refresh) | Short-lived JWTs (locally validated → fast), opaque refresh tokens (stored → revocable) |
| One user pool for all Supercluster apps | Single identity; apps get scoped access via OAuth scopes/claims |
| EF Core with Postgres + SQLite adapters | Postgres for production, SQLite for low-traffic deployments; same domain code, swappable adapter |
| Authorization Code + PKCE for pre-v1 | The secure SPA flow; client credentials and device flow deferred to backlog |

## Phases

### Phase 1: Core Domain + Passkey Auth

The minimum viable identity server: register with passkey, authenticate with passkey, recover via email.

**Status:** ✅ Completed

**What was built (actual):**

- Domain: `User` (AR), `PasskeyCredential`, `RecoveryCode` entities; `Email`, `DeviceName` value objects; 4 domain events
- Application: 10 commands + 2 queries; CQRS with `ICommandHandler`/`IQueryHandler`; assembly-scanned handlers; `ISender` mediator
- Infrastructure: EF Core with Postgres + SQLite configs; `ShadowIdGenerator` for client-side PKs; Dapper query repos; `PersistenceProvider` enum with dispatch helper
- Presentation: 11 minimal API endpoints implementing `IEndpoint`; `EndpointVersion` enum with `v1` default; RFC 7807 Problem Details via `Result<T>.ToHttpResponse()`
- Host: ASP.NET Core minimal API host; DI wired with `AddMediator()`, `AddPassportCommandsAndQueries()`, `AddPassportInfrastructure()`, `AddPassportEndpoints()`
- Tests: 42 domain + 21 application + 3 integration = 66 tests passing
- Shared libs added: `Supercluster.Lib.Application` (CQRS, mediator, providers), `Supercluster.Lib.Infrastructure` (provider impls), `Supercluster.Lib.Presentation.Http` (IEndpoint, versioning, RFC 7807)

---

#### Domain Model

##### User

```csharp
// Passport.Core.Domain/User.cs
public sealed class User : AggregateRoot
{
    // --- Identity ---
    public Guid Id { get; }
    public string Email { get; }              // normalized (lowercase, trimmed)
    public bool EmailVerified { get; private set; }

    // --- Auth ---
    public IReadOnlyCollection<PasskeyCredential> Passkeys { get; }

    // --- Timestamps ---
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
}
```

**Invariants:**
- Email is always normalized (lowercase, trimmed). Enforced in the factory.
- Email must be unique across all users (enforced at the repository level).
- After `Register`, `EmailVerified` starts as `false`. After `VerifyEmail` is called, it becomes `true`.
- A user must have at least one `PasskeyCredential`, *unless* they are currently in a recovery flow. This is enforced at the use case level — the aggregate allows zero passkeys during the recovery window.
- Equality is by email (natural domain discriminator).
- `Guid` values (`UserId`) are passed as parameters, never generated inside the domain.

##### PasskeyCredential

```csharp
// Passport.Core.Domain/PasskeyCredential.cs
public sealed class PasskeyCredential : Entity
{
    // --- WebAuthn Data ---
    public byte[] CredentialId { get; }        // WebAuthn credential ID (unique per passkey)
    public byte[] PublicKey { get; }           // stored public key for assertion verification
    public uint SignCount { get; private set; } // WebAuthn signature counter (anti-replay)

    // --- Metadata ---
    public string? DeviceName { get; }         // user-friendly label (e.g., "iPhone 15", "YubiKey")
    public DateTimeOffset CreatedAt { get; }

    // --- Ownership ---
    public Guid UserId { get; }
}
```

**Invariants:**
- `CredentialId` must be non-empty.
- `PublicKey` must be non-empty.
- `SignCount` is monotonically increasing — updated on each successful authentication via `UpdateSignCount`.
- Equality is by `CredentialId` (natural domain discriminator — the WebAuthn credential ID is the identity).
- `internal` factory — only the `User` aggregate root can create a `PasskeyCredential`.

##### RecoveryCode

```csharp
// Passport.Core.Domain/RecoveryCode.cs
public sealed class RecoveryCode : Entity, IEquatable<RecoveryCode>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private RecoveryCode() { }

    public static RecoveryCode Issue(RecoveryCodeId id, string codeHash, RecoveryCodePurpose purpose, DateTimeOffset now, TimeSpan ttl)
    {
        var recoveryCode = new RecoveryCode
        {
            Id = id,
            CodeHash = codeHash,
            Purpose = purpose,
            ExpiresAt = now + ttl,
            CreatedAt = now,
        };
        return recoveryCode;
    }

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public RecoveryCodeId Id { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public RecoveryCodePurpose Purpose { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public bool IsUsed => UsedAt.HasValue;
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(RecoveryCode? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is RecoveryCode other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();

    // ------------------------------------------------------------
    // Behaviors
    // ------------------------------------------------------------

    public void MarkUsed(DateTimeOffset now)
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Recovery code has already been used.");
        }
        UsedAt = now;
    }
}

public enum RecoveryCodePurpose
{
    EmailVerification,
    AccountRecovery,
}

public readonly record struct RecoveryCodeId(Guid Value);
```

**Invariants:**
- Code is never stored in plaintext — only `CodeHash` (SHA-256 of the generated code).
- A code can only be used once (`MarkUsed` enforces this with an exception if already used).
- `IsExpired(now)` takes `DateTimeOffset` as a parameter — no `DateTimeOffset.UtcNow` in the domain.
- Equality is by `RecoveryCodeId` (strongly-typed ID — no natural domain discriminator exists).
- `UsedAt` is null until `MarkUsed` is called.

---

#### Flows

##### 1. Registration

```
User                   SPA                  Server
 |                      |                     |
 |  enter email         |                     |
 |--------------------->|                     |
 |                      | POST /register/begin|
 |                      |  { email }          |
 |                      |-------------------->|
 |                      |                     | check email unique
 |                      |                     | generate WebAuthn options
 |                      |                     |   (challenge, rp, user handle)
 |                      |  200 { options }    |
 |                      |<--------------------|
 |  browser passkey UI  |                     |
 |<---------------------|                     |
 |  (touch/scan/etc.)   |                     |
 |--------------------->|                     |
 |                      | navigator.credentials.create()
 |                      |                     |
 |                      | POST /register/complete
 |                      |  { email, attestation }
 |                      |-------------------->|
 |                      |                     | validate attestation
 |                      |                     | create User + PasskeyCredential
 |                      |                     | generate email verification code
 |                      |                     | send verification email
 |                      |  201 { userId }     |
 |                      |<--------------------|
 |                      |                     |
 |  enter code from     |                     |
 |  email               |                     |
 |--------------------->|                     |
 |                      | POST /register/verify-email
 |                      |  { userId, code }   |
 |                      |-------------------->|
 |                      |                     | validate code hash
 |                      |                     | mark code used
 |                      |                     | set EmailVerified = true
 |                      |  200 { verified }   |
 |                      |<--------------------|
```

**Error cases:**
- `POST /register/begin`: email already registered → `409 Conflict`
- `POST /register/complete`: challenge expired / attestation invalid → `400 Validation`
- `POST /register/verify-email`: code expired / invalid / already used → `400 Validation`
- `POST /register/verify-email`: user not found → `404 NotFound`

##### 2. Authentication

```
User                   SPA                  Server
 |                      |                     |
 |  enter email         |                     |
 |--------------------->|                     |
 |                      | POST /login/begin   |
 |                      |  { email }          |
 |                      |-------------------->|
 |                      |                     | lookup user by email
 |                      |                     | get user's credential IDs
 |                      |                     | generate assertion challenge
 |                      |  200 { options,     |
 |                      |   credentialIds[] } |
 |                      |<--------------------|
 |  browser passkey UI  |                     |
 |<---------------------|                     |
 |  (touch/scan/etc.)   |                     |
 |--------------------->|                     |
 |                      | navigator.credentials.get()
 |                      |                     |
 |                      | POST /login/complete
 |                      |  { email, assertion }
 |                      |-------------------->|
 |                      |                     | lookup credential by ID
 |                      |                     | validate assertion signature
 |                      |                     | update SignCount
 |                      |                     | create session (cookie, Phase 1)
 |                      |  200 { userId }     |
 |                      |<--------------------|
```

**Error cases:**
- `POST /login/begin`: user not found → `404 NotFound`
- `POST /login/begin`: email not verified → `403 Forbidden` (can't authenticate without verified email — recovery depends on it)
- `POST /login/complete`: assertion invalid / challenge expired → `400 Validation`
- Rate limit: N failed authentication attempts within a window → temporary lockout (deferred — see Phase 5 backlog)

**Design note:** `POST /login/begin` returns credential IDs so the SPA can pass them to `navigator.credentials.get({ allowCredentials: [...] })`. This is standard WebAuthn — the server tells the browser which passkeys are allowed for this user.

##### 3. Account Recovery

Triggered when a user loses their passkey (device lost, key destroyed, etc.).

```
User                   SPA                  Server
 |                      |                     |
 |  "lost my passkey"  |                     |
 |  enter email         |                     |
 |--------------------->|                     |
 |                      | POST /recovery/begin|
 |                      |  { email }          |
 |                      |-------------------->|
 |                      |                     | lookup user by email
 |                      |                     | verify email is verified
 |                      |                     | generate recovery code
 |                      |                     | (6-digit numeric, 10min TTL)
 |                      |                     | send email with code
 |                      |  200 { maskedEmail }|
 |                      |<--------------------|
 |                      |                     |
 |  check email,        |                     |
 |  enter code          |                     |
 |--------------------->|                     |
 |                      | POST /recovery/verify-code
 |                      |  { email, code }    |
 |                      |-------------------->|
 |                      |                     | validate code hash
 |                      |                     | mark code used
 |                      |                     | return recovery token
 |                      |  200 { recoveryToken }|
 |                      |<--------------------|
 |                      |                     |
 |  create new passkey  |                     |
 |<---------------------|                     |
 |--------------------->|                     |
 |                      | POST /recovery/complete
 |                      |  { recoveryToken,   |
 |                      |    attestation }    |
 |                      |-------------------->|
 |                      |                     | validate recovery token
 |                      |                     | remove ALL existing passkeys
 |                      |                     | create new PasskeyCredential
 |                      |  200 { userId }     |
 |                      |<--------------------|
```

**Design notes:**
- Recovery removes all existing passkeys — this prevents the lost key from being a lingering security risk.
- Recovery code is numeric, 6 digits, 10-minute TTL. Rate limited: 1 request per minute per email. Max 3 code attempts.
- Recovery token is a short-lived bearer token (not a JWT — just a random opaque string, 5 min TTL) that grants the ability to complete the recovery. It's bound to the specific user + recovery session.
- The user must have a verified email to recover. Unverified accounts cannot recover — they'd just re-register.

**Error cases:**
- `POST /recovery/begin`: user not found → `200` (don't reveal existence — show "if this email is registered, we sent a code")
- `POST /recovery/begin`: email not verified → `200` (same reason)
- `POST /recovery/begin`: rate limited → `429 Too Many Requests`
- `POST /recovery/verify-code`: code invalid / expired / max attempts → `400 Validation`
- `POST /recovery/complete`: recovery token invalid / expired → `400 Validation`

---

#### Commands & Queries

All in `Passport.Core.Application/`. Commands mutate state (via EF Core command repos). Queries read state (via Dapper query repos with raw SQL).

##### Commands

| Command | Handler | Result | Notes |
|---------|---------|--------|-------|
| `RegisterUser.Begin` | `ICommandHandler<BeginRegistrationCommand, CredentialCreateOptions>` | WebAuthn options JSON | Stores challenge in `IChallengeStore` |
| `RegisterUser.Complete` | `ICommandHandler<CompleteRegistrationCommand, UserId>` | `Result<UserId>` | Validates attestation; creates User + Credential; sends verification email |
| `VerifyUserEmail` | `ICommandHandler<VerifyEmailCommand, Unit>` | `Result<Unit>` | Hashes code, compares, marks email verified |
| `AuthenticateUser.Begin` | `ICommandHandler<BeginAuthenticationCommand, AssertionOptions>` | WebAuthn assertion options | Looks up user; generates challenge with credential IDs |
| `AuthenticateUser.Complete` | `ICommandHandler<CompleteAuthenticationCommand, UserId>` | `Result<UserId>` | Validates assertion; bumps sign count |
| `RecoverAccount.Begin` | `ICommandHandler<BeginRecoveryCommand, Unit>` | `Result<Unit>` | Sends recovery code email (or silently succeeds if no user) |
| `RecoverAccount.Verify` | `ICommandHandler<VerifyRecoveryCodeCommand, string>` | `Result<string>` (recovery token) | Validates code; returns one-time recovery token |
| `RecoverAccount.Complete` | `ICommandHandler<CompleteRecoveryCommand, UserId>` | `Result<UserId>` | Removes all existing passkeys; registers new one |
| `RemovePasskey` | `ICommandHandler<RemoveCredentialCommand, Unit>` | `Result<Unit>` | Fails if it's the last credential (must use recovery) |

##### Queries

| Query | Handler | Result | Notes |
|-------|---------|--------|-------|
| `GetCredentials` | `IQueryHandler<GetCredentialsQuery, IReadOnlyList<CredentialInfo>>` | Passkey metadata | Dapper; returns read model — no domain types |
| `FindUser` | `IQueryHandler<FindUserQuery, UserReadModel>` | User read model | Dapper; lookup by email for auth flows |

---

#### Endpoints (Presentation.Http)

Controllers delegate to command/query handlers via DI:

```
POST   /api/auth/register/begin       # → BeginRegistrationCommand
POST   /api/auth/register/complete    # → CompleteRegistrationCommand
POST   /api/auth/register/verify-email # → VerifyEmailCommand
POST   /api/auth/login/begin          # → BeginAuthenticationCommand
POST   /api/auth/login/complete       # → CompleteAuthenticationCommand
POST   /api/auth/recovery/begin       # → BeginRecoveryCommand
POST   /api/auth/recovery/verify-code # → VerifyRecoveryCodeCommand
POST   /api/auth/recovery/complete    # → CompleteRecoveryCommand
GET    /api/auth/credentials          # → GetCredentialsQuery (authenticated)
DELETE /api/auth/credentials/{id}     # → RemoveCredentialCommand (authenticated)
```

All endpoints return a standard envelope: `{ success: bool, data?: T, errors?: Error[] }`.

---

#### Application Project Structure

```
Passport.Core.Application/
├── Commands/
│   ├── Registration/
│   │   ├── BeginRegistrationCommand.cs
│   │   ├── BeginRegistrationCommandHandler.cs
│   │   ├── CompleteRegistrationCommand.cs
│   │   └── CompleteRegistrationCommandHandler.cs
│   ├── Authentication/
│   │   ├── BeginAuthenticationCommand.cs
│   │   ├── BeginAuthenticationCommandHandler.cs
│   │   ├── CompleteAuthenticationCommand.cs
│   │   └── CompleteAuthenticationCommandHandler.cs
│   ├── Recovery/
│   │   ├── BeginRecoveryCommand.cs
│   │   ├── BeginRecoveryCommandHandler.cs
│   │   ├── CompleteRecoveryCommand.cs
│   │   └── CompleteRecoveryCommandHandler.cs
│   ├── Credentials/
│   │   ├── RemoveCredentialCommand.cs
│   │   └── RemoveCredentialCommandHandler.cs
│   └── Verification/
│       ├── VerifyEmailCommand.cs
│       └── VerifyEmailCommandHandler.cs
├── Queries/
│   ├── GetCredentialsQuery.cs
│   ├── GetCredentialsQueryHandler.cs
│   ├── FindUserQuery.cs
│   └── FindUserQueryHandler.cs
├── Ports/
│   ├── IChallengeStore.cs              # temp storage for WebAuthn challenges
│   ├── IEmailSender.cs                 # send verification/recovery emails
│   ├── IFido2.cs                       # WebAuthn/FIDO2 abstraction
│   ├── IUserCommandRepository.cs       # EF Core — load/save User aggregates
│   ├── IUserQueryRepository.cs         # Dapper — read user data via SQL
│   └── IRecoveryCodeRepository.cs      # manage recovery codes
└── ReadModels/
    ├── UserReadModel.cs
    └── CredentialInfo.cs
```

---

#### Infrastructure

**WebAuthn library:** [fido2-net-lib](https://github.com/passwordless-lib/fido2-net-lib) — the standard .NET FIDO2/WebAuthn library.

**Persistence:**

```
Passport.Infrastructure/
├── Data/
|   ├── PassportDbContext.cs          # EF Core DbContext
│   ├── Configurations/
│   │   ├── Postgres/
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── PasskeyCredentialConfiguration.cs
│   │   │   └── RecoveryCodeConfiguration.cs
│   │   └── Sqlite/
│   │       ├── UserConfiguration.cs
│   │       ├── PasskeyCredentialConfiguration.cs
│   │       └── RecoveryCodeConfiguration.cs
│   ├── Migrations/                   # ef migrations (auto-generated)
│   └── Repositories/
│       ├── UserRepository.cs         # IUserRepository implementation
│       └── RecoveryCodeRepository.cs
├── Auth/
│   └── WebAuthnService.cs            # wraps fido2-net-lib
├── Email/
│   ├── IEmailSender.cs               # port (in Application layer)
│   └── SmtpEmailSender.cs            # adapter (SMTP relay)
└── DependencyInjection.cs            # AddPassportInfrastructure(connectionString, provider)
```

**Database providers:**
- `AddPassportPostgres(connectionString)` — uses Npgsql
- `AddPassportSqlite(connectionString)` — uses Microsoft.Data.Sqlite

Both register the same `PassportDbContext` with the appropriate provider. The domain and application code never branches on provider.

**Email sending:** Phase 1 requires an email sender for verification and recovery codes. Use a port/adapter pattern:
- `IEmailSender` interface in `Passport.Core.Application` (port)
- `SmtpEmailSender` in infrastructure (adapter, SMTP relay)
- Optional: `LoggingEmailSender` for development (writes to console/file instead of sending)

**Challenge storage:** WebAuthn challenges are temporary — stored server-side between begin/complete calls. Options:
1. **Memory cache** (`IMemoryCache`) — simplest, but doesn't survive restarts. Fine for Phase 1.
2. **Distributed cache** (Redis) — needed for multi-instance deployments. Deferred to Phase 5.

Decision for Phase 1: use `IMemoryCache` (it's built-in, sufficient for single-instance). Abstract behind an `IChallengeStore` interface so we can swap later.

---

#### Tests

| Test Project | What |
|-------------|------|
| `tests/Passport.Core.Domain.Tests/` | Entity invariants (can't create invalid User, PasskeyCredential, RecoveryCode) |
| `tests/Passport.Core.Application.Tests/` | Command and query handler logic with mocked repositories + mock WebAuthn; test error paths |
| `tests/Passport.Infrastructure.Tests/` | EF Core mappings (can persist/retrieve User with Passkeys); WebAuthn integration with fido2-net-lib mocks |

WebAuthn mocking strategy: `fido2-net-lib` provides test helpers. Alternatively, wrap it behind an `IFido2` interface in Application and mock at that boundary. Prefer the interface approach — it keeps use case tests fast and not dependent on the library.

---

#### Acceptance Criteria (Phase 1)

- [ ] User can register with email + passkey
- [ ] User receives verification email; clicking link / entering code verifies email
- [ ] Unverified users cannot authenticate
- [ ] User can authenticate with passkey
- [ ] User can list and remove passkeys (but not the last one)
- [ ] User can recover account: email → code → create new passkey (old passkeys removed)
- [ ] Recovery silently succeeds if email not found (no user enumeration)
- [ ] Recovery code has 6 digits, 10-min TTL, max 3 attempts
- [ ] WebAuthn ceremony works in Chrome, Firefox, Safari
- [ ] EF Core migrations run against Postgres and SQLite
- [ ] All command and query handlers have tests covering happy path + error cases
- [ ] Domain entities enforce their invariants at construction time

---

### Phase 2: Session & Token Management

JWTs and refresh tokens, turning authentication into lasting sessions.

**Status:** 🔜 Not started (blocked by Phase 1)

---

#### Domain Additions

##### RefreshToken

```csharp
public sealed class RefreshToken : Entity, IEquatable<RefreshToken>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private RefreshToken() { }

    public static RefreshToken Issue(RefreshTokenId id, string tokenHash, DateTimeOffset now, TimeSpan ttl, string? clientId = null)
    {
        var refreshToken = new RefreshToken
        {
            Id = id,
            TokenHash = tokenHash,
            ClientId = clientId,
            ExpiresAt = now + ttl,
            IssuedAt = now,
        };
        return refreshToken;
    }

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public RefreshTokenId Id { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public string? ClientId { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(RefreshToken? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is RefreshToken other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();

    // ------------------------------------------------------------
    // Behaviors
    // ------------------------------------------------------------

    public void Revoke(DateTimeOffset now)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Refresh token is already revoked.");
        }
        RevokedAt = now;
    }
}

public readonly record struct RefreshTokenId(Guid Value);
```

**Configurable settings** (`appsettings.json` or equivalent):

```json
{
  "Passport": {
    "AccessToken": {
      "LifetimeMinutes": 15,
      "Issuer": "https://passport.example.com",
      "Audience": "supercluster"
    },
    "RefreshToken": {
      "LifetimeDays": 30,
      "RotationEnabled": true
    },
    "SigningKey": {
      "Algorithm": "ES256",
      "KeyPath": "/etc/passport/signing-key.pem"
    }
  }
}
```

**Key points:**
- Access token expiry is configurable. Changes take effect for tokens issued after the config change. Existing tokens are unaffected (they can't be retroactively expired since they're validated locally by services).
- Refresh tokens are stored as hashes in the DB (like recovery codes — plaintext never persisted).
- Refresh token rotation: if enabled, each refresh issues a new refresh token AND invalidates the old one. This limits the damage window of a stolen refresh token (if both the attacker and the legitimate user try to refresh, one will fail because the old token was consumed → alert/log).

#### Commands

| Command | Handler | Result | Notes |
|---------|---------|--------|-------|
| `CreateSession` | `ICommandHandler<CreateSessionCommand, (string accessToken, string refreshToken)>` | Token pair | Generates JWT + opaque refresh token; stores refresh token hash |
| `RefreshAccessToken` | `ICommandHandler<RefreshAccessTokenCommand, (string accessToken, string? newRefreshToken)>` | New token pair | Validates refresh token hash; if rotation enabled, revokes old, issues new |
| `RevokeRefreshToken` | `ICommandHandler<RevokeRefreshTokenCommand, Unit>` | `Result<Unit>` | Revokes a specific refresh token |
| `RevokeAllUserTokens` | `ICommandHandler<RevokeAllUserTokensCommand, Unit>` | `Result<Unit>` | Revokes all refresh tokens for a user ("logout everywhere") |

#### Endpoints

```
POST /api/auth/token/refresh        # → RefreshAccessToken
DELETE /api/auth/tokens             # → RevokeAllUserTokens (authenticated)
DELETE /api/auth/tokens/{id}        # → RevokeRefreshToken (authenticated)
GET /.well-known/jwks.json          # → JWKS endpoint (public key for token validation)
```

#### Acceptance Criteria (Phase 2)

- [ ] Access token (JWT) issued after authentication
- [ ] Access token expiry configurable via settings
- [ ] Refresh token stored (hashed) and exchangeable for new access token
- [ ] Refresh token rotation: old token revoked when new one issued
- [ ] JWKS endpoint serves public keys for local token validation by downstream services
- [ ] User can revoke all tokens ("logout everywhere")
- [ ] Revoked/expired refresh tokens rejected

---

### Phase 3: Frontend (SolidJS SPA)

Login, signup, and account management in `apps/passport/`.

**Pages:**
- Signup flow: enter email → create passkey (WebAuthn) → email verification
- Login flow: enter email → authenticate with passkey
- Recovery flow: enter email → receive code → create new passkey
- Dashboard: manage passkeys (list, add, remove), update recovery email
- (Post-Phase 4): OAuth consent screen

**Shared UI dependencies:**
- `apps/shared-ui/` must be scaffolded first (design tokens, Button, Input, Modal)

**Status:** 🔜 Not started (blocked by Phase 2, partially parallelizable with Phase 2 for signup/login/recovery flows)

---

### Phase 4: OAuth 2.0 / OpenID Connect

Authorization Code + PKCE with refresh tokens. The OIDC protocol surface.

**Domain additions:**
- `Client` — registered OAuth client (client_id, redirect URIs, allowed scopes)
- `AuthorizationCode` — short-lived, single-use code

**Endpoints:**
- `GET /authorize` — start authorization code flow (with PKCE challenge)
- `POST /token` — exchange authorization code for tokens, or refresh token for new access token
- `GET /.well-known/openid-configuration` — OIDC discovery
- `GET /.well-known/jwks.json` — JWT signing keys
- `GET /userinfo` — OIDC userinfo endpoint

**Commands:**
- `RegisterClient` — register a new OAuth client
- `BeginAuthorization` — validate client, scopes, generate authorization code
- `ExchangeCode` — validate code + PKCE verifier, issue tokens
- `IntrospectToken` — (optional, for opaque token support)

**Status:** 🔜 Not started (blocked by Phase 2)

---

### Phase 5+: Backlog

Deferred features — not planned in detail yet.

- **Client credentials flow** — service-to-service auth
- **Device authorization flow** — for input-constrained devices
- **Managed/admin UI** — client registration dashboard, user management
- **Multi-tenancy** — separate user pools per tenant
- **Webhooks/events** — notify downstream apps of user events (registered, email changed, deleted)
- **Brute-force protection** — rate limiting on recovery, auth endpoints
- **Audit logging** — security event logging (logins, recovery attempts, client registrations)
- **Session management UI** — user can see/revoke active sessions

---

## Acceptance Criteria

### Phase 1
- [ ] User can register with email + passkey
- [ ] User can authenticate with passkey
- [ ] User can recover account via email + code, then set up a new passkey
- [ ] WebAuthn ceremony works in Chrome, Firefox, Safari
- [ ] EF Core migrations work for both Postgres and SQLite
- [ ] All use cases covered by tests
- [ ] Domain model enforces: at least one passkey per user (unless in recovery); email must be unique

### Phase 2
- [ ] Access token (JWT) issued after authentication
- [ ] Refresh token stored and exchangeable for new access token
- [ ] JWKS endpoint serves public keys for local token validation
- [ ] Refresh token rotation (optional but recommended)
- [ ] Token expiry configurable

### Phase 3
- [ ] Signup, login, recovery flows functional in browser
- [ ] Passkey management (add/remove) functional
- [ ] Email verification flow functional
- [ ] All pages responsive and accessible
- [ ] Design tokens come from `shared-ui`

### Phase 4
- [ ] Authorization Code + PKCE flow working end-to-end
- [ ] OIDC discovery document at `/.well-known/openid-configuration`
- [ ] `/userinfo` returns claims for authenticated user
- [ ] Client registration (manual or API — TBD)
- [ ] OAuth consent screen

---

## Decision Log

| Date | Decision | Reason |
|------|----------|--------|
| 2026-06-27 | Hybrid JWT + refresh token model | Low-latency local validation for services; revocation at refresh boundary |
| 2026-06-27 | Passkeys only, no passwords | Maximize security; eliminate phishing + credential stuffing vectors |
| 2026-06-27 | Email for recovery only | Minimal signup friction; email serves as recovery channel |
| 2026-06-27 | Postgres + SQLite adapters | Postgres for production deployments; SQLite for low-traffic/single-instance |
| 2026-06-27 | Phase 1: core auth before OAuth | Auth primitives must work before the protocol layer is built on top |
| 2026-06-27 | Phases 3 and 2 can partially overlap | Frontend can prototype login/signup/recovery flows once Phase 1 use cases exist, even before tokens |