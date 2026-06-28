# Supercluster

A collection of self-hosted services, built with .NET and SolidJS.

---

## Projects

| Project | Status | Description |
|---------|--------|-------------|
| **Passport** | 🔧 Planned | Identity server — OAuth 2.0 / OpenID Connect with passkeys |
| Training | 🔜 | — |
| Budgeting | 🔜 | — |

---

## Architecture

Every service follows **Clean Architecture** across a shared monorepo:

```
src/     — C# backends (.NET 10)
apps/    — SolidJS + Tailwind frontends (SPA/PWA)
tests/   — mirrored test suite
docs/    — system of record for all design & planning
```

Shared libraries (`Supercluster.Lib.*`) provide primitives and domain base classes. Each project layers cleanly: `Domain → Application → Infrastructure → Presentation`.

See [`ARCHITECTURE.md`](ARCHITECTURE.md) for the full map.

---

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for frontend apps)
- [pnpm](https://pnpm.io/)

### Build a backend

```bash
dotnet build projects/{project}/{Project}.slnx
dotnet test projects/{project}/{Project}.slnx
```

### Run a frontend

```bash
cd apps/{app-name}
pnpm install
pnpm dev
```

---

## Design Principles

- **Parse, don't validate.** Data shapes are enforced at boundaries.
- **Errors as values.** `Result<T>` for all fallible operations.
- **Option over null.** `Option<T>`; null is never a valid return.
- **Passkeys first.** Passport uses WebAuthn exclusively — no passwords.

See [`docs/DESIGN.md`](docs/DESIGN.md) for the full philosophy.

---

## License

Apache 2.0 — see [`LICENSE`](LICENSE).
