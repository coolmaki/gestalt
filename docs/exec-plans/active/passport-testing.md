# Plan: Passport Testing

**Created:** 2026-06-28
**Status:** 🔧 Draft
**Project:** Passport
**Driving Agent:** human

## Goal

Establish test conventions and implement test suites across the Passport layers. Domain gets unit tests, Application gets unit tests with mocked ports, and a separate integration test project validates end-to-end behavior against real infrastructure.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| xUnit | Standard .NET test framework; clean DI support, `WebApplicationFactory` integration |
| NSubstitute | Cleaner API than Moq; fewer ceremony around setups/verifications |
| Testcontainers for both Postgres + SQLite | Both providers are supported in production; both must be tested |
| Podman | Container runtime used on this machine; Testcontainers supports it natively |
| `Method_Scenario_ExpectedResult` naming | Standard .NET/xUnit convention; mirrors Arrange-Act-Assert structure |
| 80%+ coverage on Domain, 70%+ on Application | Domain is deterministic and benefits most; Application has I/O boundaries |
| In-memory challenge/email for tests | Avoid real SMTP/FIDO2 dependencies in integration tests; swap implementations |

## Test Projects

### 1. Passport.Core.Domain.Tests (Unit)

Pure unit tests. No infrastructure, no mocks needed — entities and value objects are deterministic.

**What to test:**
- Entity creation via factory methods (happy path + validation errors)
- Entity behavior methods (happy path + error returns)
- Value object creation (format validation, normalization)
- Equality comparisons (domain discriminators)
- Domain immutability (private setters, collection wrappers)

**Structure:**
```
tests/Passport.Core.Domain.Tests/
├── Passport.Core.Domain.Tests.csproj
├── Entities/
│   ├── UserTests.cs
│   ├── PasskeyCredentialTests.cs
│   └── RecoveryCodeTests.cs
├── ValueObjects/
│   ├── EmailTests.cs
│   └── DeviceNameTests.cs
└── Events/
    └── DomainEventTests.cs
```

### 2. Passport.Core.Application.Tests (Unit)

Unit tests for command/query handlers. All ports (repos, services) are mocked via NSubstitute. No database, no HTTP.

**What to test:**
- Each handler: happy path + all error branches
- Handler correctly calls ports in order
- Handler returns correct Result<T> for each outcome
- Shared helpers (code generation, hashing)

**Structure:**
```
tests/Passport.Core.Application.Tests/
├── Passport.Core.Application.Tests.csproj
└── Commands/
    ├── Registration/
    │   ├── BeginRegistrationTests.cs
    │   └── CompleteRegistrationTests.cs
    ├── Authentication/
    │   ├── BeginAuthenticationTests.cs
    │   └── CompleteAuthenticationTests.cs
    ├── Recovery/
    │   ├── BeginRecoveryTests.cs
    │   ├── VerifyRecoveryCodeTests.cs
    │   ├── BeginRecoveryRegistrationTests.cs
    │   └── CompleteRecoveryTests.cs
    ├── Credentials/
    │   └── RemoveCredentialTests.cs
    └── Verification/
        └── VerifyEmailTests.cs
```

### 3. Passport.IntegrationTests

End-to-end tests using `WebApplicationFactory` with a real test host, real database via Testcontainers, and HTTP calls through `HttpClient`. Tests the full vertical slice: endpoint → mediator → handler → repository → database.

**What to test:**
- Registration flow (begin → complete → email verified)
- Authentication flow (begin → complete)
- Recovery flow (4-step)
- Credential management (list, remove)
- Error responses follow RFC 7807
- Both Postgres (Testcontainers) and SQLite (in-memory) providers

**Structure:**
```
tests/Passport.IntegrationTests/
├── Passport.IntegrationTests.csproj
├── TestHost.cs                    ← WebApplicationFactory setup
├── TestConfiguration.cs           ← Testcontainers config
├── Flows/
│   ├── RegistrationFlowTests.cs
│   ├── AuthenticationFlowTests.cs
│   ├── RecoveryFlowTests.cs
│   └── CredentialFlowTests.cs
└── Providers/
    ├── PostgresProviderTests.cs
    └── SqliteProviderTests.cs
```

**Test host setup:**
- Override `PersistenceConfiguration.Provider` and `ConnectionString` from Testcontainers
- Swap `IEmailSender` → capture emails in-memory for assertions
- Swap `IChallengeStore` → in-memory (ConcurrentDictionary)
- Swap `IFido2` → deterministic mock that returns fixed credentials

---

## Test Conventions

### General
| Convention | Detail |
|-----------|--------|
| Framework | xUnit |
| Mocking | NSubstitute |
| Naming | `Method_Scenario_ExpectedResult` |
| Structure | Mirror `src/` layout in `tests/` |
| Assertions | xUnit `Assert` + FluentAssertions for complex checks |
| Fixtures | `IClassFixture<T>` for shared setup (DB, host) |

### Naming Examples
```csharp
// Domain
public void Register_ValidEmail_CreatesUser()
public void Register_EmptyEmail_ReturnsValidationError()

// Application
public void HandleAsync_NewEmail_ReturnsOptionsJson()
public void HandleAsync_DuplicateEmail_ReturnsConflict()

// Integration
public async Task RegisterFlow_ValidInput_CompletesSuccessfully()
public async Task RegisterFlow_DuplicateEmail_ReturnsConflict()
```

### Test Project Setup
- Each test project references the project under test
- Integration tests reference the host project (`Passport.csproj`) for `WebApplicationFactory`
- Testcontainers packages: `Testcontainers.PostgreSql`, `Testcontainers.SqlEdge`
- Podman is the container runtime — Testcontainers detects it via `DOCKER_HOST` or Podman socket

### Podman
Testcontainers for .NET supports Podman. Configuration:
```csharp
Environment.SetEnvironmentVariable("DOCKER_HOST", "unix:///run/podman/podman.sock");
```
Or configure via `~/.testcontainers.properties`:
```
docker.host=unix:///run/podman/podman.sock
```

---

## Steps

1. [ ] Set up test project scaffolding (3 projects + solution references)
2. [ ] Implement `Passport.Core.Domain.Tests`
3. [ ] Implement `Passport.Core.Application.Tests`
4. [ ] Set up Testcontainers + `WebApplicationFactory` for integration tests
5. [ ] Implement `Passport.IntegrationTests`

## Acceptance Criteria

- [ ] Domain test coverage ≥ 80%
- [ ] Application test coverage ≥ 70%
- [ ] All 11 endpoints have integration test coverage (happy path + key errors)
- [ ] Both Postgres and SQLite providers pass integration tests
- [ ] Tests run in CI without external dependencies (Testcontainers for DB only)
- [ ] All tests passing with `dotnet test projects/passport/Passport.slnx`