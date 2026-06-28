---
name: coding-csharp
description: C# coding conventions for Supercluster (.NET 10). Use when writing, reviewing, or refactoring any C# code in src/ or tests/. Covers naming, structure, domain entity rules, Result/Option patterns, and formatting.
---

# C# Coding Conventions

This skill points to the authoritative docs. Load them as needed.

## Quick Reference

### Every File
- **Braces required** on all control flow (`if`, `else`, `for`, `foreach`, `while`, `lock`). `using` is the only exception.
- **Trailing commas** in multi-line lists.
- **Async suffix** on all async methods (`HandleAsync`, `SaveChangesAsync`).
- **File-scoped namespaces**.
- **`ArgumentNullException.ThrowIfNull`** for required reference params from external callers.
- **`internal` and `sealed` by default**.

### Domain Entities
Read [`docs/DESIGN.md`](../../docs/DESIGN.md) — "Domain Entity Conventions" section for full rules.

Key points:
- Private empty constructor only. Public static factory methods.
- `public` get, `private` set properties.
- Collection backing fields: `private readonly List<T> _items = [];` + `public IReadOnlyCollection<T> Items => _items.AsReadOnly();`
- Domain discriminators over IDs. `IEquatable<T>` on all entities (except records — they auto-implement it).
- Value objects for properties with invariants (Email, DeviceName).
- `Result<T>` for fallible methods. Exceptions only for null args (bugs).
- `Option<T>` for optional method params/returns. `string?` for nullable DB properties.
- Aggregate root has public behaviors. Child entities have `internal` mutators.

### File Sections (in order)
```csharp
// --- Constants --- (no header, at top)
// ------------------------------------------------------------
// Constructors & Factories
// ------------------------------------------------------------
// ...
// ------------------------------------------------------------
// Backing Fields
// ------------------------------------------------------------
// ...
// ------------------------------------------------------------
// Properties
// ------------------------------------------------------------
// ...
// ------------------------------------------------------------
// Equality
// ------------------------------------------------------------
// ...
// ------------------------------------------------------------
// Behaviors (or Methods)
// ------------------------------------------------------------
// ...
// ------------------------------------------------------------
// Internal
// ------------------------------------------------------------
// ...
```

### Project Structure
- Entities → `{Project}.Core.Domain/Entities/`
- Value objects → `{Project}.Core.Domain/ValueObjects/`
- Domain events → `{Project}.Core.Domain/Events/`

## Full Docs

- [`AGENTS.md`](../../AGENTS.md) — code conventions section
- [`ARCHITECTURE.md`](../../ARCHITECTURE.md) — layer rules, namespace conventions
- [`docs/DESIGN.md`](../../docs/DESIGN.md) — design philosophy, entity conventions, patterns
- [`.editorconfig`](../../.editorconfig) — enforced formatting rules