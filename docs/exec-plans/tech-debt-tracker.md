# Tech Debt Tracker

Known technical debt items across the monorepo, their severity, and planned remediation.

| ID | Severity | Area | Project | Description | Created |
|----|----------|------|---------|-------------|---------|
| TD-001 | ✅ Resolved | Backend | shared | `AgreggateRoot` typo fixed in `Gestalt.Lib.Domain`. Renamed to `AggregateRoot`. | 2026-06-27 |
| TD-002 | 🟡 Medium | Backend | shared | No test projects exist for `Gestalt.Lib.Primitives` or `Gestalt.Lib.Domain`. | 2026-06-27 |
| TD-003 | 🟡 Medium | CI | all | No CI/CD configuration. Build and test validation is manual. | 2026-06-27 |
| TD-004 | ✅ Resolved | Frontend | shared | `@gestalt/core` scaffolded with pnpm workspace, design tokens, 4-theme system, 7 components, PWA, and auth plans. | 2026-06-27 |
| TD-005 | ✅ Resolved | Docs | all | Test conventions documented in `docs/TESTING.md`. Covers test naming, per-layer templates, coverage targets, hybrid exclusion policy (attribute + pattern + testing), and integration test infrastructure. | 2026-06-27 |
| TD-006 | 🟡 Medium | Backend | Passport | `NoOpEmailSender` replaced with `CapturingCodeDeliveryService` for tests, but no production SMTP sender exists. Future: `Gestalt.Lib.Infrastructure.Email` with `SmtpEmailSender`. | 2026-07-09 |
| TD-007 | 🟡 Medium | Backend | Passport | `RecoveryCodeRepository.FindActiveByEmailAsync` uses EF Core LINQ with value converters that SQLite provider can't translate. Uses client-evaluation via `ToListAsync` as workaround. | 2026-07-09 |
| TD-008 | 🟢 Low | Backend | Passport | `BeginAuthenticationCommandHandler` doesn't check `EmailVerified` before returning options. Should return `403 Forbidden` for unverified users. | 2026-07-09 |
| TD-009 | 🟡 Medium | Frontend | shared | Color tokens `info`, `success`, `warning` lack hover variants (`infoHover`, `successHover`, `warningHover`). Only `primary` and `danger` have hover states. | 2026-07-10 |
| TD-010 | 🟡 Medium | E2E | Passport | No automated E2E tests. UAT checklists exist at `projects/passport/uat/` as manual tests. Automate using Playwright's .NET bindings in `tests/Passport.E2E/`. Requires Chromium virtual authenticator support for passkey flows. | 2026-08-02 |

## Severity Guide

- 🔴 **High**: Breaking change if not fixed soon; blocks progress.
- 🟡 **Medium**: Should fix; doesn't block but compounds over time.
- 🟢 **Low**: Nice to have; no urgency.

## Process

1. Add entries as they are discovered during development.
2. The doc-gardening agent scans for stale items.
3. Resolved items are moved to `docs/exec-plans/completed/`.