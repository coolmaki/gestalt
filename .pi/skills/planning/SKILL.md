---
name: planning
description: How to create and manage execution plans in Supercluster. Use when asked to plan work, create a plan, or update plan status. Covers plan template, lifecycle, and where plans live per project vs cross-cutting.
---

# Planning

This skill points to the authoritative docs. Load them as needed.

## Quick Reference

### Where Plans Live
- **Project-specific plans**: `projects/{project}/docs/plan.md`
- **Cross-cutting plans**: `docs/exec-plans/active/`
- **Completed cross-cutting plans**: `docs/exec-plans/completed/`
- **Tech debt tracker**: `docs/exec-plans/tech-debt-tracker.md`

### When to Create a Plan
1. Work spans multiple layers or projects.
2. Introduces a new architectural concept.
3. Has more than 3 distinct implementation steps.
4. Needs design discussion before coding.

### Plan Template
```markdown
# Plan: [short descriptive title]

**Created:** YYYY-MM-DD
**Status:** 🔧 Draft | 🚀 Active | ✅ Completed | ❌ Abandoned
**Project:** [project name or "shared" or "all"]
**Driving Agent:** [agent name or "human"]

## Goal
[One paragraph: what and why?]

## Design Decisions
| Decision | Rationale |
|----------|-----------|
| [choice] | [why] |

## Steps
1. [ ] Step 1 — [layer/project]
2. [ ] Step 2 — [layer/project]

## Acceptance Criteria
- [ ] Criterion 1

## Decision Log
| Date | Decision | Reason |
|------|----------|--------|
| YYYY-MM-DD | [what changed] | [why] |
```

### Lifecycle
🔧 Draft → 🚀 Active → ✅ Completed (move to completed/) 
Or ❌ Abandoned if no longer relevant.

## Full Docs

- [`docs/PLANS.md`](../../docs/PLANS.md) — planning process, scaffolding conventions
- [`docs/exec-plans/tech-debt-tracker.md`](../../docs/exec-plans/tech-debt-tracker.md) — known tech debt