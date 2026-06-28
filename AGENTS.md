# AGENTS.md

> **This is a map, not an encyclopedia.** It points agents to the right deeper sources of truth elsewhere in the repository. Keep it short (~100 lines). When in doubt, read the linked document.

---

## Quick Start

### Backend (.NET 10, C#)
- Build: `dotnet build projects/{project}/{Project}.slnx`
- Test: `dotnet test projects/{project}/{Project}.slnx`
- Formatting: `.editorconfig` at the repo root enforces all style rules
- Package manager: NuGet (standard .NET SDK)

### Frontend (SolidJS + Tailwind CSS)
- Located in `apps/`
- Package manager: `pnpm`
- Build: `pnpm build` (from the app directory)
- Dev: `pnpm dev`

---

## Repository Map

```
supercluster/                     ← monorepo root
  AGENTS.md                       ← you are here
  ARCHITECTURE.md                 ← architecture rules & dependency graph
  .editorconfig                   ← C# formatting (applies to all src/ and tests/)
  projects/
    {project}/                    ← per-project: .slnx, docs, docker, CI
  src/
    Supercluster.Lib.Primitives/  ← shared: Result<T>, Option<T>, Error, Unit
    Supercluster.Lib.Domain/      ← shared: Entity, AggregateRoot, DomainEvent
    {Project}.Core.Domain/        ← per-project: domain entities & business rules
    {Project}.Core.Application/   ← per-project: use cases, ports/interfaces
    {Project}.Infrastructure/     ← per-project: DB, external APIs, file I/O
    {Project}.Presentation.Http/  ← per-project: ASP.NET controllers, middleware
  apps/
    shared-ui/                    ← shared: design system, navigation, PWA utils
    {app}/                        ← per-app SolidJS SPA/PWA
  tests/
    Supercluster.Lib.Primitives.Tests/
    {Project}.Core.Domain.Tests/
    {Project}.Core.Application.Tests/
    ...
  projects/
    {project}/                     ← per-project: .slnx, docs, docker, CI
    passport/
      Passport.slnx
      docs/
        exec-plans/               ← project-specific execution plans
  docs/                           ← repo-wide design & planning knowledge
  projects/
    passport/
      Passport.slnx
      docs/
        exec-plans/               ← project-specific execution plans
```

---

## Architecture

All projects follow **Clean Architecture** (Uncle Bob):

```
Presentation → Application → Domain
     ↓                            ↑
  Infrastructure ─────────────────┘
```

- **Domain** (`{Project}.Core.Domain`): Entities, value objects, aggregates, domain events. Zero dependencies.
- **Application** (`{Project}.Core.Application`): Use cases, ports/interfaces. Depends only on Domain.
- **Infrastructure** (`{Project}.Infrastructure`): Implements Application ports. DB, external APIs, file I/O.
- **Presentation** (`{Project}.Presentation.Http`): ASP.NET controllers, middleware, request/response mapping.

**Shared libraries** (`Supercluster.Lib.*`) provide base types used by all projects. They are the only cross-project code dependencies.

**Cross-cutting concerns** (auth, telemetry, feature flags) enter through a single Providers/Middleware interface in the Presentation or Infrastructure layer. No domain code reaches out to infrastructure directly.

See [`ARCHITECTURE.md`](./ARCHITECTURE.md) for the full map, namespace conventions, and enforced invariants.

---

## Design Principles

- **Parse, don't validate**: Data shapes are validated at boundaries (controllers, API clients).
- **Errors as values**: `Result<T>` for fallible operations; exceptions only for truly unexpected failures.
- **Option over null**: `Option<T>` for optional values; null is never a valid return.
- **Immutability by default**: Records with `{ get; init; }`; mutable state only when necessary.

See [`docs/DESIGN.md`](./docs/DESIGN.md) for the full design philosophy.
See [`docs/FRONTEND.md`](./docs/FRONTEND.md) for frontend conventions.

---

## Plans & Tracking

- Active execution plans: [`docs/exec-plans/active/`](./docs/exec-plans/active/) (cross-cutting)
- Per-project plans: `projects/{project}/docs/exec-plans/active/`
- Completed plans: [`docs/exec-plans/completed/`](./docs/exec-plans/completed/)
- Known technical debt: [`docs/exec-plans/tech-debt-tracker.md`](./docs/exec-plans/tech-debt-tracker.md)
- Per-project plans and specs: create under `projects/{project}/docs/` for project-specific plans.
- Cross-cutting plans: `docs/exec-plans/active/` for work spanning multiple projects.

---

## How to Add a New Project

1. Choose a project name (PascalCase, e.g. `Passport`, `Training`, `Budgeting`).
2. Scaffold the .NET solution in `projects/{project}/{Project}.slnx` with projects:
   - `{ProjectName}.Core.Domain`
   - `{ProjectName}.Core.Application`
   - `{ProjectName}.Infrastructure`
   - `{ProjectName}.Presentation.Http`
3. If the project has a frontend, scaffold it in `apps/{project-name}/` (kebab-case) with SolidJS + Tailwind.
4. Create test projects in `tests/` mirroring the backend project structure.
5. Create an execution plan in `docs/exec-plans/active/` if the work spans multiple steps.

---

## Code Conventions

### C# (all projects)
- `_camelCase` for private instance fields, `s_camelCase` for private static fields.
- File-scoped namespaces (`namespace Foo.Bar;`).
- Explicit types (no `var`).
- **Null guards.** Use `ArgumentNullException.ThrowIfNull()` for required reference parameters from external callers.
- Private constructors + public static factory methods for sum types (`Result<T>`, `Option<T>`).
- XML doc comments on all public APIs.
- Section separators: `// --- Section Name ---`.
- **Braces required.** All control flow statements (`if`, `else`, `for`, `foreach`, `while`, `lock`) must use braces — even for single-statement bodies. `using` statements are the only exception where braces may be omitted. No other exceptions.
- **Trailing commas.** Use trailing commas in multi-line lists: enums, array/collection initializers, object initializers, parameter lists, and switch expressions.
- **Async suffix.** All async methods use the `Async` suffix (`HandleAsync`, `SaveChangesAsync`, etc.).
- **Types are `internal` and `sealed` by default.** Only make a type `public` when it is explicitly needed by another package. Only make a type `unsealed` when inheritance is explicitly part of its design. Prefer composition over inheritance.
- **Result, not exceptions.** Domain and Application methods return `Result<T>` for expected failures. Exceptions are for truly unexpected conditions only.
- **Option<T> for optional parameters and returns.** Use `Option<T>` in method signatures when a value may legitimately be absent. Use `string?` (nullable reference types) for entity properties that map to nullable DB columns.

### Domain Projects (`{Project}.Core.Domain`)
- **Entities** → `Entities/` (namespace: `{Project}.Core.Domain.Entities`)
- **Value objects** → `ValueObjects/` (namespace: `{Project}.Core.Domain.ValueObjects`)
- **Domain events** → `Events/` (namespace: `{Project}.Core.Domain.Events`)

### SolidJS / TypeScript
- See [`docs/FRONTEND.md`](./docs/FRONTEND.md).

---

## Context

See [`docs/references/`](./docs/references/) for external references and design inspirations.
