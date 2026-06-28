# ARCHITECTURE.md

Top-level map of the monorepo, dependency rules, and namespace conventions. This document defines the structural invariants all projects must follow.

---

## Monorepo Layout

```
supercluster/
├── src/
│   ├── Supercluster.Lib.Primitives/       ← shared: foundational types
│   ├── Supercluster.Lib.Domain/           ← shared: DDD base classes
│   ├── Supercluster.Lib.Application/      ← shared: commands/queries interfaces, providers
│   ├── Supercluster.Lib.Infrastructure/   ← shared: provider implementations
│   ├── Passport.Core.Domain/              ← project: domain layer
│   ├── Passport.Core.Application/         ← project: application layer
│   ├── Passport.Infrastructure/           ← project: infrastructure layer
│   └── Passport.Presentation.Http/        ← project: presentation layer
├── apps/
│   ├── shared-ui/                         ← shared: design system, nav, PWA
│   └── passport/                          ← project: SolidJS SPA/PWA
├── tests/
│   ├── Supercluster.Lib.Primitives.Tests/
│   ├── Passport.Core.Domain.Tests/
│   ├── Passport.Core.Application.Tests/
│   └── ...
├── projects/
│   └── passport/                           ← project: .slnx, README, CI/CD
│       └── Passport.slnx
└── docs/                                  ← repo-wide design & planning
```

---

## Clean Architecture (All Projects)

Every project follows the same layered architecture:

```
┌─────────────────────────────────────────────┐
│              Presentation.Http               │  ← controllers, middleware, serialization
│                   (↑)                        │
│              Application                     │  ← commands/queries, ports (interfaces), read models
│                   (↑)                        │
│              Domain                          │  ← entities, value objects, domain events
└─────────────────────────────────────────────┘
         ↑                           ↑
         │     Infrastructure        │         ← DB, external APIs, file I/O, email
         └───────────────────────────┘
```

### Dependency Rule

**Source code dependencies point inward.** Outer layers depend on inner layers. Inner layers know nothing about outer layers.

- `Domain` depends on **nothing** (except `Supercluster.Lib.Primitives` for `Result<T>`, `Option<T>`, etc.).
- `Application` depends on `Domain`. Defines interfaces (ports) that `Infrastructure` implements.
- `Infrastructure` depends on `Application` (implements its interfaces). Also depends on external packages (EF Core, HttpClient, etc.).
- `Presentation.Http` depends on `Application`. Wires up DI, maps HTTP to commands/queries.

### Cross-Cutting Concerns

Authentication, authorization, telemetry, and feature flags enter through:

1. **Middleware** (in `Presentation.Http`) — for HTTP-level concerns (auth cookies, correlation IDs).
2. **Providers** (in `Infrastructure`) — injected into Application via DI, implementing Application-defined interfaces.
3. **No direct dependency** from Domain or Application on infrastructure packages.

---

## Namespace Conventions

### Shared Libraries

| Namespace | Purpose |
|-----------|---------|
| `Supercluster.Lib.Primitives` | `Result<T>`, `Option<T>`, `Error`, `ErrorType`, `Unit` |
| `Supercluster.Lib.Domain` | `Entity`, `AggregateRoot`, `DomainEvent` |

### Project Namespaces

Per-project namespaces use the pattern `{ProjectName}.{Layer}`:

| Layer | Namespace Pattern | Example (Passport) |
|-------|-------------------|---------------------|
| Domain | `{Project}.Core.Domain` | `Passport.Core.Domain` |
| Application | `{Project}.Core.Application` | `Passport.Core.Application` |
| Infrastructure | `{Project}.Infrastructure` | `Passport.Infrastructure` |
| Presentation | `{Project}.Presentation.Http` | `Passport.Presentation.Http` |

---

## Frontend Architecture (`apps/`)

### Technology Stack
- **Framework**: SolidJS
- **Styling**: Tailwind CSS
- **Package manager**: pnpm
- **Build tool**: Vite

### Shared Frontend Packages (`apps/shared-ui/`)

Reusable across all frontend apps:

| Package | Purpose |
|---------|---------|
| Design system | Colors, fonts, spacing, component library |
| Navigation | Custom stack navigator for SPA/PWA routing |
| PWA utilities | Service worker, manifest, offline support |

### Per-App Structure (`apps/{app-name}/`)

```
apps/passport/
├── src/
│   ├── components/        ← UI components
│   ├── pages/             ← route-level page components
│   ├── hooks/             ← shared reactive logic
│   ├── signals/           ← global state (SolidJS signals/stores)
│   ├── api/               ← API client layer (typed fetch wrappers)
│   └── index.tsx          ← entry point
├── public/                ← static assets, PWA manifest
├── tailwind.config.ts
├── vite.config.ts
└── package.json
```

Apps communicate with the backend through a typed API client layer (`api/`). No direct database access from the frontend.

---

## Enforced Invariants

These are mechanically enforced (linters, structural tests, build checks):

### Backend
1. **No circular dependencies.** The dependency graph must remain acyclic.
2. **Shared libraries have zero project references** (except Primitives, which has none at all).
3. **No infrastructure code in Domain or Application.** No `IO`, no `HttpClient`, no database access in inner layers.
4. **All public types are documented** with XML doc comments.
5. **`Result<T>` for all fallible operations** in Application and Domain layers.
6. **Interfaces in Application, implementations in Infrastructure.** The dependency inversion principle.

### Frontend
1. **API calls go through the `api/` layer.** No raw `fetch` in components.
2. **Shared design tokens in `shared-ui/`.** No duplicated colors/fonts/spacing across apps.
3. **Components are self-contained.** One component per file; co-locate styles with Tailwind classes.

### Cross-Cutting
1. **One `.slnx` per project** — in `projects/{project}/`.
2. **Tests mirror `src/` layout** — `tests/{Project}.{Layer}.Tests/`.

---

## Current Projects

| Project | Status | Backend | Frontend |
|---------|--------|---------|----------|
| Passport | 🚀 Phase 1 Active | `src/Passport.Core.*` | `apps/passport/` |
| Training | 🔜 Planned | — | — |
| Budgeting | 🔜 Planned | — | — |

---

## Shared Libraries (Current)

| Library | Depends On | Provides |
|---------|-----------|----------|
| `Supercluster.Lib.Primitives` | *nothing* | `Result<T>`, `Option<T>`, `Error`, `ErrorType`, `Unit` |
| `Supercluster.Lib.Domain` | Primitives | `Entity`, `AggregateRoot`, `DomainEvent` |
| `Supercluster.Lib.Application` | Primitives | `ICommand<T>`, `ICommandHandler`, `IQuery<T>`, `IQueryHandler`, providers |
| `Supercluster.Lib.Infrastructure` | Application | `DateTimeProvider`, `GuidProvider`, DI extensions |
