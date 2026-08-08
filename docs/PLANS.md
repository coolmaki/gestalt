# PLANS.md

Planning process and conventions for execution plans across all projects in the monorepo.

---

## When to Create an Execution Plan

Create an execution plan in `docs/exec-plans/active/` when:

1. The change spans **multiple layers** or **multiple projects**.
2. The change introduces a **new architectural concept** (new domain, new layer, new pattern).
3. The change has **more than 3 distinct implementation steps**.
4. The change needs **design discussion** before coding begins.
5. You are **scaffolding a new project**.

For smaller changes, a lightweight plan inline in the PR description is sufficient.

---

## Execution Plan Template

```markdown
# Plan: [short descriptive title]

**Created:** YYYY-MM-DD
**Status:** 🔧 Draft | 🚀 Active | ✅ Completed | ❌ Abandoned
**Project:** [Passport | Training | Budgeting | shared | all]
**Driving Agent:** [agent name or "human"]

## Goal

[One paragraph: what are we building and why? How does this serve the project's purpose?]

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| [choice] | [why] |

## Steps

1. [ ] Step 1 — [layer/project affected]
2. [ ] Step 2 — [layer/project affected]
3. [ ] Step 3 — [layer/project affected]

## Acceptance Criteria

- [ ] Criterion 1
- [ ] Criterion 2

## Decision Log

| Date | Decision | Reason |
|------|----------|--------|
| YYYY-MM-DD | [what changed] | [why] |
```

---

## Lifecycle

1. **Draft** (`🔧`) — Plan is being written and discussed.
2. **Active** (`🚀`) — Plan is being implemented.
3. **Completed** (`✅`) — All steps done, criteria met. Move to `completed/`.
4. **Abandoned** (`❌`) — No longer relevant. Move to `completed/` with a note.

---

## Active Plans

See [`docs/exec-plans/active/`](./exec-plans/active/) for all active execution plans.

## Completed Plans

See [`docs/exec-plans/completed/`](./exec-plans/completed/) for archived plans.

---

## Scaffolding a New Project

When starting a new project in the monorepo:

1. **Create an execution plan** in `docs/exec-plans/active/` with:
   - Project name and purpose
   - Which layers are needed (always Domain + Application + Infrastructure + Presentation.Http for backends)
   - Whether a frontend is needed
   - Key domain concepts

2. **Scaffold the solution:**
   - Create `projects/{project}/{ProjectName}.slnx`
   - Create backend projects in `src/`: `{ProjectName}.Core.Domain`, `{ProjectName}.Core.Application`, `{ProjectName}.Infrastructure`, `{ProjectName}.Presentation.Http`
   - Create test projects in `tests/`: `{ProjectName}.Core.Domain.Tests`, etc.
   - If frontend: create `apps/{project-name}/` with SolidJS + Tailwind scaffold

3. **Wire dependencies:**
   - All projects reference `Gestalt.Lib.Primitives`
   - Domain projects reference `Gestalt.Lib.Domain`
   - Application references Domain
   - Infrastructure references Application
   - Presentation.Http references Application

4. **Add to quality tracking:**
   - Add project row to `docs/QUALITY_SCORE.md`
   - Add project to the project table in `ARCHITECTURE.md`
