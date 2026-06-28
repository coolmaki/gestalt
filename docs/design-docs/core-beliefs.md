# Core Beliefs

These are the foundational, non-negotiable principles that guide all design decisions across every project in the Supercluster monorepo. When facing a tradeoff, resolve it in favor of these beliefs.

---

## 1. Agents Are First-Class Contributors

The codebase is optimized for legibility by coding agents (Codex, Aardvark, etc.). Human engineers steer; agents execute.

**Implications:**
- Documentation lives in the repository, not in external tools (Google Docs, Slack, Notion).
- Architecture is enforced mechanically (linters, structural tests), not by convention.
- Code is self-describing: types, patterns, and conventions are consistent and predictable across all projects.
- External dependencies are chosen for API stability and composability.

## 2. Make Invalid States Unrepresentable

If a state shouldn't exist, the type system should make it impossible to express.

**Implications:**
- `Result<T>` for operations that can fail — the error path is part of the signature.
- `Option<T>` for values that may be absent — null is never a valid return.
- Private constructors + static factory methods to enforce invariants at construction time.
- `{ get; init; }` properties — set once at construction, immutable thereafter.

## 3. Clean Architecture, Consistently

Every project follows the same layered dependency structure: Domain → Application → Infrastructure → Presentation. The dependency rule is absolute.

**Implications:**
- Inner layers never reference outer layers. Period.
- Interfaces (ports) are defined in Application; implementations (adapters) in Infrastructure.
- Shared libraries (`Supercluster.Lib.*`) provide base types only — no business logic.
- New projects are scaffolded identically; the structure is not open for interpretation.

## 4. Progressive Disclosure

Anything not accessible in-context to an agent effectively doesn't exist. Start small; teach the agent where to look next.

**Implications:**
- `AGENTS.md` is a map (~100 lines), not an encyclopedia.
- Deeper knowledge is organized in `docs/` with clear indexing.
- Per-project specs and plans live under `docs/product-specs/` and `docs/exec-plans/`.

## 5. Taste Is Encoded, Not Remembered

Human preferences and quality standards are captured in tooling, lints, and tests so they apply continuously across all projects.

**Implications:**
- Custom linters enforce naming, file size, layering, and documentation rules.
- Lint error messages include remediation instructions for agents.
- "Doc-gardening" runs scan for stale documentation.
- Review feedback that reveals a pattern is promoted into an automated check.

## 6. Small, Continuous Cleanup Over Large Refactors

Technical debt is paid down in small daily increments, not painful quarterly bursts — across the entire monorepo.

**Implications:**
- Background agent tasks scan for deviations from golden principles.
- Quality grades per project are tracked in `docs/QUALITY_SCORE.md`.
- Every PR leaves the codebase slightly better than it found it.

## 7. Monorepo, Unified Standards

All projects share the same conventions, tools, and quality bar. The monorepo is the system of record for everything.

**Implications:**
- One `.editorconfig` governs all C# code.
- Shared libraries, shared frontend components, shared docs.
- Per-project solutions in `projects/{project}/`; per-project tests in `tests/`.
- New projects start from the same template, not from ad-hoc decisions.
