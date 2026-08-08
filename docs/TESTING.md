# TESTING.md

Test conventions, coverage targets, and exclusion policy for all projects in the Gestalt monorepo.

---

## Test Framework & Tools

| Tool | Purpose |
|------|---------|
| xUnit | Unit and integration test framework |
| NSubstitute | Mocking library for Application-layer handler tests |
| coverlet | Code coverage collection (`dotnet test --collect:"XPlat Code Coverage"`) |

### Running Tests

```bash
# All tests
dotnet test projects/{project}/{Project}.slnx

# With coverage collection
dotnet test projects/{project}/{Project}.slnx --settings tests/coverlet.runsettings --collect:"XPlat Code Coverage"

# Single provider (integration tests)
PASSPORT_TEST_PROVIDER=sqlite dotnet test
PASSPORT_TEST_PROVIDER=postgres dotnet test
```

### Coverage Settings

`coverlet.runsettings` defines per-layer thresholds and exclusion filters. See the file at `tests/coverlet.runsettings`.

---

## Test Naming

`Method_Scenario_ExpectedResult` — follows Arrange-Act-Assert structure.

```
// Domain
Register_ValidEmail_CreatesUser
Issue_EmptyCodeHash_ReturnsValidationError

// Application
HandleAsync_ValidAssertion_ReturnsSessionResult
HandleAsync_UserNotFound_ReturnsNotFound

// Integration
RegisterFlow_ValidInput_ReturnsSuccess
LoginFlow_UnverifiedEmail_ReturnsForbidden
```

### Test Projects

Tests mirror the `src/` layout in `tests/`:

```
tests/
├── {Project}.Core.Domain.Tests/       ← unit tests (pure, no mocks)
├── {Project}.Core.Application.Tests/  ← unit tests (mocked ports via NSubstitute)
└── {Project}.IntegrationTests/        ← end-to-end (WebApplicationFactory + SQLite/Postgres)
```

---

## Per-Layer Test Templates

### Domain Entity Tests

Every entity and value object must have its own test file. Standard test suite:

| Category | Tests Needed |
|----------|-------------|
| Factory happy path | 1 per factory |
| Factory validation errors | 1 per guard clause |
| Behavior/mutation | 1 per public method |
| State guard errors | 1 per error branch |
| Equality — same identity | 1 |
| Equality — different identity | 1 |
| GetHashCode consistency | 1 |
| Expiry/time (if applicable) | 2 (before + after boundary) |
| Event raising | 1 per event raised |
| Side effects on parent | 1 per AR method |

Target: **95% line coverage**.

### Application Handler Tests

Every command/query handler must have its own test file. **Use Type A (field-based DI) exclusively:**

```csharp
public class FooCommandHandlerTests
{
    private readonly IDependency1 _dep1 = Substitute.For<IDependency1>();
    private readonly IDependency2 _dep2 = Substitute.For<IDependency2>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly FooCommandHandler _handler;

    public FooCommandHandlerTests()
    {
        _clock.UtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _handler = new FooCommandHandler(_dep1, _dep2, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidInput_ReturnsSuccess() { ... }

    [Fact]
    public async Task HandleAsync_ErrorCondition_ReturnsExpectedError() { ... }
}
```

Standard test suite:

| Category | Tests Needed |
|----------|-------------|
| Happy path | 1 |
| Input validation errors | 1 per value-object boundary |
| Not-found errors | 1 per `Option.None` branch |
| Domain error propagation | 1 per `domainResult.IsFailure` branch |
| Conflict/state errors | 1 per unique business rule rejection |
| SaveChanges assertion | Must assert `userRepo.Received(1).SaveChangesAsync(...)` on every happy path |

**Infrastructure failures (FIDO2, email, token) are exercised by integration tests, not unit tests with mocks.**

Target: **75% line coverage**.

### Integration Tests

Full vertical slices: endpoint → mediator → handler → repository → database.

| Category | Tests Needed |
|----------|-------------|
| Happy path | Every endpoint: 200 with correct body shape |
| Primary error path | Every endpoint: most common failure mode |
| Critical flows | Multi-step flows (auth, recovery, token refresh) |
| Auth-gated endpoints | 1 with JWT + 1 without |

**Infrastructure: All repositories, services, and EF configurations are exercised exclusively through integration tests.** No dedicated unit tests for infrastructure types.

Target: **60% Infrastructure, 70% Presentation** (via integration test execution).

---

## Coverage Targets

| Layer | Target | Rationale |
|-------|--------|-----------|
| Domain | ≥ 95% | Pure deterministic code; 100% achievable |
| Application | ≥ 75% | Some plumbing lines cannot be independently asserted |
| Infrastructure | ≥ 60% | EF configuration boilerplate counts as lines but isn't "executed" |
| Presentation | ≥ 70% | ASP.NET framework middleware not counted |
| **Overall** | **≥ 80%** | |

---

## Coverage Exclusion Policy (Hybrid Approach)

The project uses a **hybrid** exclusion strategy. Types are classified by whether they contain business logic:

### Decision Flowchart

```
Does the type contain business logic?
├── Yes → WRITE TESTS
│   (entities, handlers, helpers, services, value objects with validation)
│
└── No → Is it a pure data carrier?
    ├── Yes → Is it likely to grow logic later?
    │   ├── Yes → WRITE TESTS (domain events)
    │   └── No → EXCLUDE
    │       (config POCOs, enums, record structs, read models, DTOs)
    │
    └── No → WRITE TESTS
```

### Method A: `[ExcludeFromCodeCoverage]` Attribute

**Use for:** Structurally static types unlikely to ever contain logic.

```csharp
[ExcludeFromCodeCoverage]
public sealed class ApplicationConfiguration { ... }
```

| When to Use | Examples |
|------------|----------|
| Configuration POCOs | `ApplicationConfiguration`, `AccessTokenConfiguration`, `InfrastructureConfiguration` |
| Enums with no methods | `RecoveryCodePurpose` |
| Pure record structs | `RecoveryCodeId` |

If a type grows logic later, the attribute acts as a gate — removing it forces a conscious decision to add tests.

### Method B: `coverlet.runsettings` Pattern Exclusions

**Use for:** Categories of types that follow consistent naming conventions.

```xml
<Exclude>[Passport.Core.Application]*Command</Exclude>
<Exclude>[Passport.Core.Application]*Query</Exclude>
<Exclude>[Passport.Core.Application]*Result</Exclude>
<Exclude>[Passport.Core.Application]*ReadModel</Exclude>
```

| Pattern | Matches |
|---------|---------|
| `*Command` | All `ICommand<T>` implementations |
| `*Query` | All `IQuery<T>` implementations |
| `*Result` | All result DTOs (`SessionResult`, etc.) |
| `*ReadModel` | All read model types |
| `*Request` | All request DTOs |

### Method C: No Exclusion — Test It

**Use for:** Types that carry data worth verifying, even if they appear to be "just records."

| Type | Why Test It |
|------|------------|
| Domain events | Events carry payloads (email addresses, timestamps). Assert that events are raised with the correct data. |
| Value objects with validation | `Email`, `DeviceName` — contain `Result<T>` factory methods with guard clauses. |
| Helpers | `GenerateCode()`, `HashCode()` — crypto utilities. |
| `JwksKey` | Carries signing key data that must be correct. |

---

## Integration Test Infrastructure

### TestHost

`TestHost : WebApplicationFactory<Program>` swaps production services for test doubles:

| Production | Test Replacement |
|-----------|-----------------|
| `IFido2` | `TestFido2Service` (deterministic fake) |
| `ICodeDeliveryService` | `CapturingCodeDeliveryService` (captures sent codes) |
| `PersistenceConfiguration` | SQLite file or Postgres container (configurable via `PASSPORT_TEST_PROVIDER` env var) |

### Provider Selection

```bash
# SQLite (default)
PASSPORT_TEST_PROVIDER=sqlite dotnet test

# Postgres (requires Podman/Docker)
PASSPORT_TEST_PROVIDER=postgres dotnet test
```

### Capturing Code Delivery

Use `_host.GetLastCode()` to read the verification/recovery code sent by the handler during a test flow. The `CapturingCodeDeliveryService` is registered as a singleton in `TestHost.ConfigureWebHost`.

```csharp
await _client.PostAsJsonAsync("/api/v1/auth/register/complete", ...);
var code = _host.GetLastCode();
Assert.NotNull(code);
```

---

## References

- [DESIGN.md](./DESIGN.md) — design philosophy, entity conventions, quality standards
- [passport-testing.md](./exec-plans/active/passport-testing.md) — original Passport testing strategy
- `tests/coverlet.runsettings` — coverage thresholds and exclusions
- `tests/Passport.IntegrationTests/TestHost.cs` — integration test template
