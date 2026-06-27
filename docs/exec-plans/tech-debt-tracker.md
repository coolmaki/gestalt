# Tech Debt Tracker

Known technical debt items across the monorepo, their severity, and planned remediation.

| ID | Severity | Area | Project | Description | Created |
|----|----------|------|---------|-------------|---------|
| TD-001 | ✅ Resolved | Backend | shared | `AgreggateRoot` typo fixed in `Supercluster.Lib.Domain`. Renamed to `AggregateRoot`. | 2026-06-27 |
| TD-002 | 🟡 Medium | Backend | shared | No test projects exist for `Supercluster.Lib.Primitives` or `Supercluster.Lib.Domain`. | 2026-06-27 |
| TD-003 | 🟡 Medium | CI | all | No CI/CD configuration. Build and test validation is manual. | 2026-06-27 |
| TD-004 | 🔴 High | Frontend | shared | `apps/shared-ui/` not yet scaffolded. Design tokens, shared components, navigation, and PWA utilities need to exist before any app frontend is built. | 2026-06-27 |
| TD-005 | 🟡 Medium | Docs | all | No test convention documented for C# (xUnit vs NUnit vs MSTest). | 2026-06-27 |

## Severity Guide

- 🔴 **High**: Breaking change if not fixed soon; blocks progress.
- 🟡 **Medium**: Should fix; doesn't block but compounds over time.
- 🟢 **Low**: Nice to have; no urgency.

## Process

1. Add entries as they are discovered during development.
2. The doc-gardening agent scans for stale items.
3. Resolved items are moved to `docs/exec-plans/completed/`.