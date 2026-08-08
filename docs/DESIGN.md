# DESIGN.md

Design philosophy, patterns, conventions, and quality standards for all projects in the Gestalt monorepo.

---

## Core Design Philosophy

Gestalt follows **functional Clean Architecture**: Uncle Bob's dependency rule combined with functional-programming patterns in an object-oriented language. The goal is to make invalid states unrepresentable and failure modes explicit.

### 1. Parse, Don't Validate

Data is validated to a known shape at system boundaries (HTTP controllers, API clients). Once inside, code assumes the shape is correct. This eliminates scattered defensive checks.

> See: [Parse, don't validate](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/)

### 2. Errors as Values (Not Exceptions)

All expected failure modes are represented as `Result<T, Error>`. Exceptions are reserved for truly unexpected failures (hardware faults, runtime bugs).

```csharp
// Good: explicit failure path in the signature
public Result<Order> PlaceOrder(PlaceOrderCommand command) { ... }

// Avoid: exceptions for expected business failures
public Order PlaceOrder(PlaceOrderCommand command) { ... }
```

### 3. Option over Null

Null is never a valid return value. Use `Option<T>` for values that may or may not exist.

```csharp
// Good: explicit absence
public Option<Customer> FindCustomer(CustomerId id) { ... }

// Avoid: null-returning methods
public Customer? FindCustomer(CustomerId id) { ... }
```

### 4. Immutable Data Types

Domain types (entities, value objects, events) use `record` for value semantics and immutability. Properties use `{ get; private set; }`. Mutable state is allowed only when a clear performance or interop requirement demands it — and is always clearly documented.

### 5. CQRS: Commands and Queries

Every operation goes through the Application layer via a command or a query. This separates writes from reads.

- **Commands** mutate state. They are handled by `ICommandHandler<TCommand, TResult>` and use EF Core command repositories to load aggregates, invoke domain logic, and persist changes. Commands return `Result<TResult>`.
- **Queries** read state. They are handled by `IQueryHandler<TQuery, TResult>` and use Dapper query repositories with raw SQL against read models. Queries return `Result<TResult>` — errors-as-values applies to reads too.

No external CQRS framework (MediatR, etc.). Handlers are wired manually via DI.

```csharp
// Command: create a user
public sealed record RegisterUserCommand(string Email, byte[] Attestation) : ICommand<UserId>;

public sealed class RegisterUserCommandHandler(
    IUserCommandRepository users,
    IFido2 fido2,
    IGuidProvider guids,
    IDateTimeProvider clock
) : ICommandHandler<RegisterUserCommand, UserId>
{
    public async Task<Result<UserId>> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {
        // validate, create entity, persist
    }
}

// Query: find a user
public sealed record FindUserQuery(string Email) : IQuery<UserReadModel>;

public sealed class FindUserQueryHandler(
    IUserQueryRepository users
) : IQueryHandler<FindUserQuery, UserReadModel>
{
    public async Task<Result<UserReadModel>> HandleAsync(FindUserQuery query, CancellationToken ct)
    {
        // raw SQL via Dapper → read model
    }
}
```

**Controllers delegate to commands/queries.** They contain no business logic — just request mapping, handler dispatch, and response mapping.

```csharp
public sealed class AuthController(
    ICommandHandler<RegisterUserCommand, UserId> registerUser,
    IQueryHandler<FindUserQuery, UserReadModel> findUser
) : ApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(request.Email, request.Attestation);
        return await registerUser.HandleAsync(command, ct)
            .Match(Ok, this.ToErrorResponse);
    }
}
```

**Repository split:**
- `IUserCommandRepository` — EF Core. `Add(User)`, `Update(User)`, `FindById(id)`, `SaveChanges()`.
- `IUserQueryRepository` — Dapper. `FindByEmail(email)`, `ListCredentials(userId)`. Returns read models, never domain entities.

---

## Error Taxonomy

Every error carries a machine-readable `Code` and a human-readable `Description`.

| ErrorType | HTTP Mapping | When to Use |
|-----------|-------------|-------------|
| `Validation` | 400 | Input failed rules — malformed data, missing fields, out of range |
| `NotFound` | 404 | Resource/entity doesn't exist (legitimate caller-handleable absence) |
| `Conflict` | 409 | Well-formed request but can't be applied now (duplicate, concurrency) |
| `Unauthorized` | 401/403 | Caller lacks permission (authN or authZ) |
| `Unexpected` | 500 | Fallback for wrapping infrastructure exceptions at boundaries |

---

## Pattern: Match/Map/Bind

All sum types (`Result<T>`, `Option<T>`) expose three operations that keep the happy path linear:

- **Match**: Branch on the variant, producing a value of type `TNext`.
- **Map**: Transform the inner value if present/success, preserving the wrapper.
- **Bind**: Chain to another operation that returns the same wrapper type.

```csharp
// Chaining without nesting
Result<User> CreateUser(string email) =>
    ValidateEmail(email)          // Result<Email>
        .Bind(e => CheckUnique(e)) // Result<Email>
        .Bind(e => Persist(e));    // Result<User>
```

---

## Repository Conventions

### C# Source Files

Source files use visual section separators for consistent internal structure. Sections appear in this order (where applicable):

```csharp
// --- Constants ---
// (no header — constants go at the top before any section headers)

// ------------------------------------------------------------
// Constructors & Factories
// ------------------------------------------------------------

// ------------------------------------------------------------
// Backing Fields
// ------------------------------------------------------------

// ------------------------------------------------------------
// Properties
// ------------------------------------------------------------

// ------------------------------------------------------------
// Equality
// ------------------------------------------------------------

// ------------------------------------------------------------
// Behaviors (or Methods, depending on context)
// ------------------------------------------------------------

// ------------------------------------------------------------
// Internal
// ------------------------------------------------------------
```

**Async method naming:** All async methods use the `Async` suffix (e.g., `HandleAsync`, `FindByEmailAsync`, `SaveChangesAsync`).

### Domain Entity Conventions

All domain entities (in `{Project}.Core.Domain`) follow these rules:

- **Return `Result<T>`, not exceptions.** All public and internal methods that can fail return `Result<T>` (or `Result<Unit>` for void operations). Exceptions are reserved for truly unexpected conditions — null arguments passed by buggy callers, not business rule violations.

  ```csharp
  public Result<Unit> RemovePasskey(byte[] credentialId, DateTimeOffset now) { ... }
  public Result<Unit> MarkUsed(DateTimeOffset now) { ... }
  ```

- **Null guards + `Option<T>` split.** Method parameters use `ArgumentNullException.ThrowIfNull` for required references. Optional parameters use `Option<T>`. Entity properties mapped to nullable DB columns use `string?` (not `Option<T>`) — EF Core understands nullable reference types natively without value converters.

  ```csharp
  // Method parameter: required = null guard, optional = Option<T>
  public Result<Unit> SomeMethod(string required, Option<string> optional) { ... }

  // Entity property: nullable = string?
  public DeviceName? DeviceName { get; private set; }
  ```

- **Value objects for invariants.** Properties that carry validation rules (format, length, character constraints) should be modeled as value objects. The value object enforces its own invariants at construction and returns `Result<T>`.

  Candidates: `Email`, `DeviceName`, any user-provided string with constraints.

- **Private empty constructor only.** A single `private Foo() { }` serves both as the EF Core materialization entry point and as the construction path for factory methods. No other constructors.

- **Public static factory methods.** The only way to create an entity from outside is through a static factory method. The factory calls the private constructor, sets properties, enforces invariants, and raises domain events.

  ```csharp
  public static Result<User> Register(Email email, DateTimeOffset now)
  {
      var user = new User
      {
          Email = email,
          EmailVerified = false,
          CreatedAt = now,
          UpdatedAt = now,
      };
      user.RaiseEvent(new UserRegistered(email.Value, now));
      return user;
  }
  ```

- **Properties: `public` get, `private` set.** EF Core can set private setters via reflection. The domain uses the factory method to set them.

  ```csharp
  public Email Email { get; private set; } = null!;
  public bool EmailVerified { get; private set; }
  public DateTimeOffset CreatedAt { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }
  ```

- **Pure/deterministic domain.** `Guid.CreateVersion7()`, `DateTimeOffset.UtcNow`, `Random`, etc. are never called in domain projects. All such values are passed as parameters to factory methods and behaviors.

- **Collection properties use private backing fields.** EF Core populates the backing field; the domain exposes a read-only wrapper.

  ```csharp
  // Backing Fields
  private readonly List<PasskeyCredential> _passkeys = [];

  // Properties
  public IReadOnlyCollection<PasskeyCredential> Passkeys => _passkeys.AsReadOnly();
  ```

- **Domain-specific discriminators over IDs.** Prefer natural domain identifiers for equality. IDs are a persistence concern and should only be used when no natural discriminator exists. Strongly-typed IDs (`FooId`) are a last resort.

  ```csharp
  // Natural discriminator:
  public bool Equals(User? other) => other is not null && Email == other.Email;

  // No natural discriminator → strongly-typed ID fallback:
  public readonly record struct RecoveryCodeId(Guid Value);
  public bool Equals(RecoveryCode? other) => other is not null && Id == other.Id;
  ```

- **Aggregate roots own public methods. Child entities are `internal`.** Only the aggregate root exposes public behaviors. Child entity methods are `internal` (or `private`) and invoked exclusively by the aggregate root. This prevents Application-layer code from mutating child entities directly.

  ```csharp
  // Aggregate root (User) — public behavior:
  public Result<PasskeyCredential> AddPasskey(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
  {
      var passkeyResult = PasskeyCredential.Create(credentialId, publicKey, signCount, now);
      if (passkeyResult.IsFailure)
      {
          return passkeyResult.Error;
      }

      var passkey = passkeyResult.Value;
      _passkeys.Add(passkey);
      RaiseEvent(new PasskeyAdded(Email.Value, credentialId, now));
      return passkey;
  }

  // Child entity (PasskeyCredential) — internal factory:
  internal static PasskeyCredential Create(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
  {
      // validate, create, return
  }
  ```

- **Domain entity files live in `Entities/`** within the domain project root. Namespace: `{Project}.Core.Domain.Entities`.
- **Value objects live in `ValueObjects/`** within the domain project root. Namespace: `{Project}.Core.Domain.ValueObjects`.
- **Domain events live in `Events/`** within the domain project root. Namespace: `{Project}.Core.Domain.Events`.

- **IEquatable<T> on all entities.** Every entity implements `IEquatable<T>` and overrides `Equals(object)` / `GetHashCode()`. The equality check uses the domain discriminator (natural key or strongly-typed ID), never the persistence ID.

- **Navigation properties over FK ID properties.** When an entity needs to reference another entity, use a navigation property (e.g., `User` property on `PasskeyCredential`), not a foreign key ID property (e.g., `UserId`). Only add navigation properties if the entity actually needs to navigate to the related entity in its business logic.

  ```csharp
  // Good: navigation property
  public User User { get; private set; } = null!;

  // Avoid: FK ID property unless actually needed in domain logic
  public Guid UserId { get; private set; }
  ```

### Extension Methods

- Extension methods must reside in `Extensions/` at the project root.
- The containing class must be `static` and named `<TypeBeingExtended>Extensions`.

  ```csharp
  // Gestalt.Lib.Infrastructure/Extensions/ServiceCollectionExtensions.cs
  public static class ServiceCollectionExtensions
  {
      public static IServiceCollection AddProviders(this IServiceCollection services) { ... }
  }
  ```

### EF Core Entity Configuration

- Every entity and aggregate root must have a corresponding `IEntityTypeConfiguration<T>` implementation.
- One configuration per EF Core backend provider (Postgres, SQLite, etc.).
- Configuration classes live in `{Project}.Infrastructure/Data/Configurations/{Provider}/`.
- All persistence concerns (relations, indexes, column types, conversions, shadow properties) live in these configuration classes — never in the domain entity itself. The domain model is persistence-ignorant.

  ```
  Passport.Infrastructure/
  ├── Data/
  │   ├── Configurations/
  │   │   ├── Postgres/
  │   │   │   ├── UserConfiguration.cs
  │   │   │   ├── PasskeyCredentialConfiguration.cs
  │   │   │   └── RecoveryCodeConfiguration.cs
  │   │   └── Sqlite/
  │   │       ├── UserConfiguration.cs
  │   │       ├── PasskeyCredentialConfiguration.cs
  │   │       └── RecoveryCodeConfiguration.cs
  │   └── PassportDbContext.cs
  ```

### C# Type Visibility & Sealing

- **`internal` by default.** Types should be `internal` unless they are explicitly needed by another package in the monorepo. This keeps the public surface area small and intentional.
- **`sealed` by default.** Classes should be `sealed` unless inheritance is an explicit, designed extension point. Prefer composition over inheritance wherever it makes sense.
- **Braces required.** All control flow statements (`if`, `else`, `for`, `foreach`, `while`, `lock`) must use braces — even for single-statement bodies. `using` statements are the only exception where braces may be omitted.
- **Trailing commas.** Use trailing commas in multi-line lists: enums, array/collection initializers, object initializers, parameter lists, and switch expressions.

### Frontend (SolidJS)

See [`docs/FRONTEND.md`](./FRONTEND.md) for all frontend conventions.

### Presentation (HTTP)

- **One class per endpoint.** Each API endpoint is its own class implementing `IEndpoint`. No controller-style grouping.
- **API versioning.** Use the `EndpointVersion` enum. `MapEndpoints()` groups by version and prefixes `/api/{version}`.
- **Error responses.** Use `Result<T>.ToHttpResponse()` which returns <see href="https://datatracker.ietf.org/doc/html/rfc7807">RFC 7807 Problem Details</see>.
- **DI registration.** `Add{Project}Endpoints()` in `Extensions/ServiceCollectionExtensions.cs` scans the assembly for `IEndpoint` implementations.

---

## Directory & File Naming

### C# Projects

- Project directories: `{ProjectName}.{Layer}` (e.g. `Passport.Core.Domain`)
- Test directories: `{ProjectName}.{Layer}.Tests`
- One `.csproj` per project; one `.slnx` per deliverable project in `projects/{project}/`

### Frontend Apps

- App directories: kebab-case in `apps/` (e.g. `apps/passport/`, `apps/budgeting-app/`)
- Shared packages: `apps/shared-ui/`

---

## Quality Standards

| Aspect | Standard |
|--------|----------|
| Public API docs | XML doc comments on all public types and members (C#) |
| Null-safety | Nullable reference types enabled; no null returns |
| Immutability | `{ get; private set; }` on properties; `readonly` fields |
| Error handling | `Result<T>` for fallible ops; `Option<T>` for optional values |
| CQRS | All state changes via commands; all reads via queries |
| Test coverage | Target >80% on Domain and Application layers. See [TESTING.md](./TESTING.md) for conventions and coverage exclusion policy. |
| Frontend types | TypeScript throughout; typed API client layer |
