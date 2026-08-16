# Plan: Theme System — Phase 4: Theme Validation Test Updates

**Created:** 2026-08-16
**Status:** ✅ Completed
**Project:** Gestalt (cross-project)
**Depends on:** `theme-system-p2-builtin-themes` (completed), lazy-load refactor (Phase 3, completed)

## Goal

Rebuild the theme validation test suite for the new architecture. After Phase 3 replaced JSON Schema + ajv with TypeScript types and token-css-vars pipeline, the deleted `theme-schema.test.ts` needed a replacement that validates the 4 built-in themes are structurally complete and that the token → CSS variable conversion is correct.

## Scope

- `tests/themes.test.ts` — validate all 4 built-in themes against the `ThemeTokens` contract
- Verifies: 69 colors (hex), 5 shadows (valid dims), 1 font family, 1 border width per theme
- Verifies `tokensToCssVars` output: exactly 76 vars, kebab-case naming, `--theme-` prefix, focus alpha (50%)
- Verifies consistent var name sets across all themes
- Round-trip: generated CSS files match `tokensToCssVars` output (catches drift from `generate-themes`)

## Files Changed

```
web/@gestalt/core/tests/themes.test.ts  ← new (22 tests)
```

## Acceptance Criteria

- [x] All 4 built-in themes pass structural validation (69 colors, 5 shadows, 1 font, 1 border)
- [x] `tokensToCssVars` produces exactly 76 CSS vars per theme
- [x] Var naming is kebab-case with `--theme-` prefix
- [x] Focus states end with "80" (50% alpha)
- [x] All 4 themes produce identical var name sets
- [x] Round-trip: each theme's generated `.css` file matches `tokensToCssVars` output
- [x] `pnpm test` passes (77 tests), `pnpm build` succeeds