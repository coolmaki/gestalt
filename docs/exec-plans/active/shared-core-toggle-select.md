# Plan: Toggle component + Select search/source/limit

**Created:** 2026-08-16
**Status:** 🚀 Active
**Project:** shared (`@gestalt/core`)

## Goal

Add a `Toggle` switch component for boolean fields, upgrade `Select` with search, async `source`, and a rendering `limit`, and replace dev-gallery checkboxes with Toggle.

## Design Decisions

| Decision | Rationale |
|---|---|
| Toggle = native checkbox + `role="switch"` | Free keyboard/AT support; no focus-trap or ARIA reinvention |
| CSS-drawn switch (not icon pack `toggle-on/off`) | Smooth slide animation; clean focus/disabled state styling |
| Controlled `checked` + `onChange` | Mirrors `Input`/`Select`; no hidden internal state |
| Single default size (no `size` prop) | Keep v1 minimal; add variants if needed later |
| `source` overrides `options` (not merged) | One source of truth per mode |
| Debounce with `createEffect` + `onCleanup` + request-id guard | No external deps; prevents stale responses clobbering newer queries |
| `limit` is client-side "Show N more" | No silent data loss; keeps `source` a pure query→options function |
| Cache `selectedLabel` with `source` | Trigger button never blanks when selected value is stale vs fetched list |

## Steps

1. [ ] `Toggle` component + `index.ts` + public API export
2. [ ] `Toggle` tests
3. [ ] `Select`: `searchable` (local filter) + search box UI
4. [ ] `Select`: `source` (debounced, loading + empty states, cached selected label)
5. [ ] `Select`: `limit` + "Show N more"
6. [ ] Extend `Select` tests
7. [ ] Demos: `ToggleDemo.tsx` + update `SelectDemo.tsx` + replace checkboxes (ButtonDemo, InputDemo, FormFieldDemo) + register in `dev/App.tsx`
8. [ ] Verify: `pnpm test`, `pnpm build`, `pnpm dev`

## Acceptance Criteria

- [ ] `Toggle` renders label, toggles on click, respects `disabled`, has `role="switch"` + `aria-checked`
- [ ] `Select` with `searchable` filters options as you type; empty list shows "No results"
- [ ] `Select` with `source` fetches on open + debounced query; shows loading + empty states
- [ ] `Select` with `limit` caps and reveals via "Show N more"
- [ ] Dev gallery checkboxes replaced by Toggles (ButtonDemo, InputDemo, FormFieldDemo)
- [ ] Existing Select usages (theme/radius pickers) behave identically
- [ ] `pnpm test` passes, `pnpm build` succeeds
