# Passport

Identity server — OAuth 2.0 / OpenID Connect with passkeys (WebAuthn) as the exclusive authentication method.

**Status:** ✅ Phase 1 Complete — 66 tests passing

## Quick Start

```bash
dotnet build projects/passport/Passport.slnx
dotnet test projects/passport/Passport.slnx
```

## Code

```
src/Passport.Core.Domain/     ← Domain entities, value objects, events
src/Passport.Core.Application/ ← Commands, queries, ports
src/Passport.Infrastructure/   ← EF Core, WebAuthn, email
src/Passport.Presentation.Http/ ← API endpoints
```

## What's Built

- [x] Domain entities: `User`, `PasskeyCredential`, `RecoveryCode`
- [x] Value objects: `Email`, `DeviceName`, `RecoveryCodeId`
- [x] Domain events: `UserRegistered`, `EmailVerified`, `PasskeyAdded`, `PasskeyRemoved`
- [x] Application layer: 10 commands + 2 queries + mediator
- [x] Infrastructure: EF Core (Postgres + SQLite), Dapper, challenge store
- [x] HTTP API: 11 endpoints with v1 versioning
- [x] Tests: 66 passing (42 domain + 21 application + 3 integration)

## Plan

See [`docs/exec-plans/active/passport-identity-server.md`](../../docs/exec-plans/active/passport-identity-server.md).