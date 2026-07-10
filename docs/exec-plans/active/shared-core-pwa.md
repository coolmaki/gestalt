# Plan: Shared Core Package — PWA Utilities

**Created:** 2026-07-10
**Status:** 🔜 Not started
**Project:** Supercluster (cross-project)
**Driving Agent:** human
**Depends on:** Plan 2 (`shared-core-design-tokens`), Plan 3 (`shared-core-components` for AppSettings)

## Goal

Build PWA infrastructure and the configurable app settings page. Every Supercluster app uses this to register its service worker, generate a manifest, and provide users with a settings page for themes and auto-update behavior.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Settings stored in localStorage | No backend needed. Theme + auto-update preference are client-only. |
| Service worker registration wrapper | Simple `registerSW()` function. Apps can opt into auto-update or manual. |
| Manifest helper | Generate manifest from typed config object. Avoid hand-writing JSON. |
| Offline-first per-app config | Pass an `offlineStrategy` option to `registerSW()`. Per-app decision. |
| App settings page as a component | `<AppSettings>` renders a pre-built settings page using core components. |

## Steps

1. [ ] Create `web/@supercluster/core/src/pwa/` directory
2. [ ] Create `src/pwa/service-worker.ts`:
   - `registerSW(options: { autoUpdate?: boolean })` — registers service worker
   - Auto-update mode: worker auto-installs on new version
   - Manual mode: emits `onUpdateAvailable` callback
   - Expose `unregisterSW()` for cleanup
3. [ ] Create `src/pwa/manifest.ts`:
   - `generateManifest(config)` — returns full manifest JSON
   - Config type: `{ name, shortName, themeColor, backgroundColor, icons, orientation?, display? }`
4. [ ] Create `src/pwa/app-settings.tsx`:
   - `<AppSettings>` using core components
   - Auto-update toggle: "Auto" / "Manual"
   - Theme selection: dropdown of available themes from `ThemeProvider`
5. [ ] Write Vitest tests for `manifest.ts` and `service-worker.ts`
6. [ ] Write component test for `<AppSettings>`
7. [ ] Export from barrel

## Acceptance Criteria

- [ ] `registerSW({ autoUpdate: true })` installs auto-updating service worker
- [ ] `generateManifest(config)` returns valid manifest JSON
- [ ] `<AppSettings>` renders theme selector and auto-update toggle
- [ ] Theme change from settings page applies via ThemeProvider
- [ ] Auto-update preference persisted to localStorage