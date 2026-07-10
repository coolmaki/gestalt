# QUALITY_SCORE.md

Per-project quality grades. Updated by the doc-gardening agent and during code review.

## Grading Rubric

| Grade | Meaning |
|-------|---------|
| 🟢 A | Excellent — everything below applies |
| 🟡 B | Good — minor gaps in one category |
| 🟠 C | Needs work — multiple gaps or a significant one |
| 🔴 D | Poor — blocking issues present |
| ⚫ F | None — no code yet, or needs complete rewrite |

### Assessed Categories (Backend)

1. **Test Coverage**: % of public API paths covered
2. **Documentation**: XML docs on public types/members; design docs current
3. **Architectural Compliance**: Dependency rules followed; no layer violations
4. **Error Handling**: `Result<T>`/`Option<T>` used consistently; no null returns; no swallowed errors
5. **Immutability**: Properties with private setters; no mutable static state

### Assessed Categories (Frontend)

1. **Test Coverage**: Component and hook test coverage
2. **Documentation**: JSDoc on exports; FRONTEND.md conventions followed
3. **Component Architecture**: One component per file; props interfaces; no inline styles
4. **API Layer Discipline**: All network calls through `api/`; no raw fetch in components
5. **Design Tokens**: Colors/fonts/spacing from `shared-ui`; no hardcoded values

---

## Current Grades

### Shared Libraries

| Library | Tests | Docs | Arch. Compliance | Error Handling | Immutability | Overall |
|---------|:---:|:---:|:---:|:---:|:---:|:---:|
| `Supercluster.Lib.Primitives` | ⚫ F | 🟢 A | 🟢 A | 🟢 A | 🟢 A | 🟠 C |
| `Supercluster.Lib.Domain` | ⚫ F | 🟡 B | 🟢 A | 🟢 A | 🟢 A | 🟠 C |

### Projects

| Project | Status | Backend | Frontend |
|---------|--------|---------|----------|
| Passport | 🚀 Phase 1 Complete | `src/Passport.Core.*` — 66 tests | `apps/passport/` |
| Training | 🔜 Planned | — | — |
| Budgeting | 🔜 Planned | — | — |

### Shared Frontend

| Package | Tests | Docs | Component Arch. | API Discipline | Design Tokens | Overall |
|---------|:---:|:---:|:---:|:---:|:---:|:---:|
| `@supercluster/core` | ⚫ F | ⚫ F | ⚫ F | N/A | ⚫ F | ⚫ F |

---

### Notes

- **Primitives**: Now has 42 domain tests passing. Internal constructors on Option<T>/Result<T> for mock compat.
- **Domain**: Doc coverage slightly lower (no XML comments on Entity/AggregateRoot/DomainEvent). AggreggateRoot typo was fixed.
- **@supercluster/core**: Not yet scaffolded.
