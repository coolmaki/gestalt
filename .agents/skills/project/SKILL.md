---
name: project
description: Map of Supercluster projects. Use when asked to work on a specific project — look up its README, location, status, and build commands. First stop before any project-specific task.
---

# Projects

Supercluster is a monorepo. Each project in `projects/{name}/` has a `README.md` with project-specific details.

## Project Registry

| Project | Directory | Status | README |
|---------|-----------|--------|--------|
| Passport | `projects/passport/` | 🚀 Phase 1 Active | [`README.md`](../../projects/passport/README.md) |

## How to Work on a Project

1. Look up the project in the registry above.
2. Load the project's `README.md` for details (what it does, current phase, code locations).
3. Load the plan from `docs/exec-plans/active/` for the current execution plan and next steps.
4. Check the tech debt tracker: [`docs/exec-plans/tech-debt-tracker.md`](../../docs/exec-plans/tech-debt-tracker.md).

## Project File Conventions

```
projects/{name}/
├── README.md          ← project overview, status, quick start
├── {Name}.slnx        ← .NET solution file
└── ci/                ← CI/CD configs (future)
```

## Build Commands

```bash
# Build a project
dotnet build projects/{name}/{Name}.slnx

# Run tests
dotnet test projects/{name}/{Name}.slnx
```

## Adding a New Project

1. Create `projects/{name}/` with `README.md`, `{Name}.slnx`, and `docs/plan.md`.
2. Add the project to the registry table in this skill's SKILL.md.
3. Add the project to the quality tracker: [`docs/QUALITY_SCORE.md`](../../docs/QUALITY_SCORE.md).
4. Add the project to the architecture doc: [`ARCHITECTURE.md`](../../ARCHITECTURE.md).

## Full Docs

- [`README.md`](../../README.md) — repo overview
- [`ARCHITECTURE.md`](../../ARCHITECTURE.md) — architecture rules, project list
- [`docs/QUALITY_SCORE.md`](../../docs/QUALITY_SCORE.md) — per-project quality grades