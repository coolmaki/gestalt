# DESIGN.md

Design philosophy, patterns, conventions, and quality standards for all projects in the Supercluster monorepo.

---

## Core Design Philosophy

Supercluster follows **functional Clean Architecture**: Uncle Bob's dependency rule combined with functional-programming patterns in an object-oriented language. The goal is to make invalid states unrepresentable and failure modes explicit.

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

Domain types (entities, value objects, events) use `record` for value semantics and immutability. Properties use `{ get; init; }`. Mutable state is allowed only when a clear performance or interop requirement demands it — and is always clearly documented.

### 5. Use Cases as the Application Boundary

Every operation that changes state goes through a **use case** (command/query handler) in the Application layer. Controllers delegate to use cases; they contain no business logic.

```csharp
// Application layer: use case
public sealed class PlaceOrderUseCase(IOrderRepository orders, IUnitOfWork uow)
{
    public async Task<Result<Order>> Execute(PlaceOrderCommand command, CancellationToken ct)
    {
        // business orchestration lives here
    }
}

// Presentation layer: controller — thin, no logic
public sealed class OrdersController(PlaceOrderUseCase placeOrder) : ApiController
{
    public async Task<IActionResult> Place(PlaceOrderRequest request, CancellationToken ct)
    {
        return await placeOrder.Execute(request.ToCommand(), ct)
            .Match(Ok, this.ToErrorResponse);
    }
}
```

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

### Domain Entity Conventions

All domain entities (in `{Project}.Core.Domain`) follow these rules:

- **Private empty constructor only.** A single `private Foo() { }` serves both as the EF Core materialization entry point and as the construction path for factory methods. No other constructors.

- **Public static factory methods.** The only way to create an entity from outside is through a static factory method. The factory calls the private constructor, sets properties, enforces invariants, and raises domain events.

  ```csharp
  public static User Register(string email, Guid userId, DateTimeOffset now)
  {
      // validate
      var user = new User
      {
          Id = new UserId(userId),
          Email = email.ToLowerInvariant().Trim(),
          CreatedAt = now,
          UpdatedAt = now,
      };
      user.RaiseEvent(new UserRegistered(user.Id, user.Email, now));
      return user;
  }
  ```

- **Properties: `public` get, `private` set.** EF Core can set private setters via reflection. The domain uses the factory method to set them.

  ```csharp
  public UserId Id { get; private set; }
  public string Email { get; private set; } = string.Empty;
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

- **Domain-specific discriminators over IDs.** Prefer natural domain identifiers for equality. IDs are a persistence concern and should only be used when no natural discriminator exists.

  ```csharp
  // Natural discriminator:
  public bool Equals(User? other) => other is not null && Email == other.Email;

  // No natural discriminator → strongly-typed ID fallback:
  public readonly record struct SomeEntityId(Guid Value);
  public bool Equals(SomeEntity? other) => other is not null && Id == other.Id;
  ```

- **Aggregate roots own public methods. Child entities are `internal`.** Only the aggregate root exposes public behaviors. Child entity methods are `internal` (or `private`) and invoked exclusively by the aggregate root. This prevents Application-layer code from mutating child entities directly.

  ```csharp
  // Aggregate root (User) — public behavior:
  public PasskeyCredential AddPasskey(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
  {
      var passkey = PasskeyCredential.Create(credentialId, publicKey, signCount, now);
      _passkeys.Add(passkey);
      RaiseEvent(new PasskeyAdded(Id, passkey.CredentialId, now));
      return passkey;
  }

  // Child entity (PasskeyCredential) — internal factory:
  internal static PasskeyCredential Create(byte[] credentialId, byte[] publicKey, uint signCount, DateTimeOffset now)
  {
      // validate, create, return
  }
  ```

- **IEquatable<T> on all entities.** Every entity implements `IEquatable<T>` and overrides `Equals(object)` / `GetHashCode()`. The equality check uses the domain discriminator (natural key or strongly-typed ID), never the persistence ID.

### C# Type Visibility & Sealing

- **`internal` by default.** Types should be `internal` unless they are explicitly needed by another package in the monorepo. This keeps the public surface area small and intentional.
- **`sealed` by default.** Classes should be `sealed` unless inheritance is an explicit, designed extension point. Prefer composition over inheritance wherever it makes sense.
- **Braces required.** All control flow statements (`if`, `else`, `for`, `foreach`, `while`, `lock`) must use braces — even for single-statement bodies. `using` statements are the only exception where braces may be omitted.
- **Trailing commas.** Use trailing commas in multi-line lists: enums, array/collection initializers, object initializers, parameter lists, and switch expressions.

### Frontend (SolidJS)

See [`docs/FRONTEND.md`](./FRONTEND.md) for all frontend conventions.

---

## Directory & File Naming

### C# Projects

- Project directories: `{ProjectName}.{Layer}` (e.g. `Passport.Core.Domain`)
- Test directories: `{ProjectName}.{Layer}.Tests`
- One `.csproj` per project; one `.slnx` per deliverable project in `solutions/`

### Frontend Apps

- App directories: kebab-case in `apps/` (e.g. `apps/passport/`, `apps/budgeting-app/`)
- Shared packages: `apps/shared-ui/`

---

## Quality Standards

| Aspect | Standard |
|--------|----------|
| Public API docs | XML doc comments on all public types and members (C#) |
| Null-safety | Nullable reference types enabled; no null returns |
| Immutability | `{ get; init; }` on properties; `readonly` fields |
| Error handling | `Result<T>` for fallible ops; `Option<T>` for optional values |
| Use case pattern | All state changes go through explicit use case classes |
| Test coverage | Target >80% on Domain and Application layers |
| Frontend types | TypeScript throughout; typed API client layer |
