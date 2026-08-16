# Plan: Theme System — Phase 2: Built-in Theme Rebuild

**Created:** 2026-08-09
**Status:** ✅ Completed
**Project:** Gestalt (cross-project)
**Depends on:** `theme-system-p1-schema` (Phase 1)
**Design doc:** `docs/design-docs/theme-system.md`

## Goal

Rebuild all 4 built-in themes from 22 flat color strings to the full 76-token shape (`ColorTokens` + `ShadowTokens` + `TypographyTokens` + `BorderTokens`). Update the Tailwind plugin to emit CSS custom properties for all token categories, and expand `styles.css` to map them into Tailwind's `@theme` namespace. By the end of this phase, switching between built-in themes reflects all 76 token dimensions in the UI.

## Scope

- `themes.ts` — replace `LegacyColorTokens` with `ColorTokens`; each theme gets full colors (nested `ColorStateTokens`), shadows, typography, borders
- `color-utils.ts` — new file: algorithmic color state derivation helper
- `plugin.ts` — expand from flat color iteration to recursive token flattening across all 4 categories
- `styles.css` — expand `@theme` from ~22 color mappings to 69 colors + typography + 5 shadows; import 3 new font packages
- `theme-provider.tsx` — update `ThemeConfig` → `ThemeTokens` type references
- `index.ts` — remove `LegacyColorTokens` export, add any new exports
- Font packages — install `@fontsource/jetbrains-mono`, `@fontsource/fira-code`, `@fontsource/ibm-plex-mono`

Out of scope: component class updates (Phase 8), custom theme CSS generation (Phase 3), schema validation test updates (Phase 4).

## Design Decisions

| Decision | Rationale |
|---|---|
| `ThemeConfig` merged into `ThemeTokens` | Same shape. No reason for two interfaces. |
| Color states derived algorithmically, with per-role overrides | Manual 120-hex-value data entry is error-prone. Derive hover/active/focus/disabled from default values using HSL manipulation, allow explicit overrides for roles that already have custom hover colors (primary, danger). |
| `secondary` role seeded from a muted complementary tone | New role not present in existing themes. Each theme defines an explicit `secondary.default` value; states derived from it. |
| `overlay` defined explicitly per theme | Dark themes: `#00000080` (50% black). Light themes: `#00000033` (20% black). |
| Global `disabled`/`disabledContent` CSS vars retained (mapped to `primaryDisabled`) | Components currently use `bg-disabled text-disabled-content`. Changing all components is Phase 8. For now, map old global disabled to primary's disabled state so components keep working. |
| `overlay` mapped to `--color-overlay` in `@theme` | Prepares for Modal component update in Phase 8. |
| Plugin flattens nested `ColorStateTokens` into individual CSS vars | `primary.default` → `--theme-color-primary`, `primary.hover` → `--theme-color-primary-hover`, etc. Structural colors and other categories are already flat. |
| Shadow DTCG objects → CSS shorthand in plugin | `{ offsetX, offsetY, blur, spread, color }` → `"0px 4px 12px 0px #00000026"` |
| Font packages imported via CSS `@import` | `@fontsource` packages ship ready-to-use `@font-face` rules. No manual `@font-face` needed. |

## Color Derivation Strategy

A new `color-utils.ts` file exports `deriveColorStates(baseHex, contentHex, isDarkTheme, overrides?)`:

```
default:     baseHex              (unchanged)
hover:       lighten(base, 10%)   (dark theme) / darken(base, 10%)  (light theme)
active:      lighten(base, 15%)   (dark theme) / darken(base, 15%)  (light theme)
focus:       baseHex with 50% alpha (for focus-visible rings)
disabled:    desaturate(base, 50%) + adjust lightness toward neutral
content roles follow the same pattern, but with the content hex as base.
```

## Steps

### 1. Install font packages
### 2. Create `color-utils.ts`
### 3. Rebuild `themes.ts`
### 4. Rewrite `plugin.ts`
### 5. Rewrite `styles.css`
### 6. Update `theme-provider.tsx`
### 7. Update `index.ts`
### 8. Update `theme-schema.test.ts` — validate built-in themes
### 9. Verify

## Files Changed

```
web/@gestalt/core/src/design/color-utils.ts       ← new
web/@gestalt/core/src/design/themes.ts             ← full rebuild
web/@gestalt/core/src/design/plugin.ts             ← recursive flattening
web/@gestalt/core/src/styles.css                   ← expanded @theme + font imports
web/@gestalt/core/src/theme-provider.tsx            ← ThemeConfig → ThemeTokens
web/@gestalt/core/src/index.ts                     ← remove LegacyColorTokens
web/@gestalt/core/tests/theme-schema.test.ts        ← built-in theme validation
web/@gestalt/core/package.json                     ← 3 font packages
```

## Acceptance Criteria

- [ ] 3 font packages installed (`@fontsource/jetbrains-mono`, `@fontsource/fira-code`, `@fontsource/ibm-plex-mono`)
- [ ] `color-utils.ts` exports `deriveColorStates` with hex/HSL utilities
- [ ] All 4 themes are valid `ThemeTokens` objects with 69 colors (nested), 5 shadows, 1 font, 1 border
- [ ] Plugin emits CSS custom properties for all 76 tokens under each `[data-theme]` selector
- [ ] `styles.css` maps all 76 tokens into Tailwind's `@theme` namespace
- [ ] `styles.css` imports all 4 monospace font packages
- [ ] `ThemeProvider` uses `ThemeTokens` type throughout
- [ ] `LegacyColorTokens` removed from public API
- [ ] Schema validation test confirms all 4 built-in themes pass
- [ ] `pnpm test` passes (61+ tests), `pnpm build` succeeds
- [ ] Theme switching between all 4 themes works visually
- [ ] No component class changes (Phase 8 scope)