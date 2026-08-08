# Plan: Shared Core Package — Design Tokens

**Created:** 2026-07-10
**Status:** ✅ Completed
**Project:** Gestalt (cross-project)
**Driving Agent:** human
**Depends on:** Plan 1 (`shared-core-scaffold`)

## Goal

Define the visual foundation of all Gestalt apps: design tokens, multi-theme CSS variable system, Geist Mono font, Tailwind v4 plugin for theme injection, and ThemeProvider. By the end of this plan, importing `@gestalt/core/styles.css` gives any app a consistent visual baseline.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| CSS custom properties for theming | `data-theme` attribute on `<html>` switches CSS variables. No JS needed for theme change — instant, no flash. |
| Token layer in TypeScript | `tokens.ts` defines the `ColorTokens` interface — a typed shape for all color slots. `themes.ts` provides 4 themes implementing it. Source of truth for values. |
| `@theme inline` + `addBase` split | Plugin injects CSS custom properties (`--theme-color-*`) per `[data-theme]`. `@theme inline` in CSS maps them to Tailwind utility names (`--color-*: var(--theme-color-*)`). Single responsibility, matches existing budget-tool pattern. |
| 4 named themes via `data-theme` | `[data-theme="obsidian"]`, `[data-theme="matrix"]`, `[data-theme="pearl"]`, `[data-theme="vapor"]` selectors switch CSS custom properties. App-extensible — add more themes by registering in the `themes` record. |
| Geist Mono via `@fontsource/geist-mono` | Variable weight font, open source, one package. Apps get the font through core's dependency. |
| Theme persist in localStorage | `ThemeProvider` writes to `localStorage` on change, reads on mount. No flash of wrong theme. |

## Steps

1. [ ] Create `web/@gestalt/core/src/design/` directory
2. [ ] Create `src/design/tokens.ts`:
    - Define `ColorTokens` interface with 22 color slots:
      - Surface: `surface`, `surfaceContent`, `surfaceAlt`, `surfaceAltContent`
      - Primary: `primary`, `primaryContent`, `primaryHover`, `primaryHoverContent`
      - Semantic: `info`, `infoContent`, `success`, `successContent`, `warning`, `warningContent`, `danger`, `dangerContent`, `dangerHover`, `dangerHoverContent`
      - Text emphasis: `highEmphasis`, `mediumEmphasis`, `lowEmphasis`
      - `border`
    - All use `-content` convention for text-on-color variants
3. [ ] Create `src/design/themes.ts`:
    - `ThemeKey` type: `"obsidian" | "matrix" | "pearl" | "vapor"`
    - 4 named themes with lowercase keys, display names via `ThemeConfig.name`
    - Each implements `ColorTokens` with hex values from the approved palette
4. [ ] Create `src/design/plugin.ts`:
    - Tailwind v4 plugin that imports `themes.ts`
    - Uses `addBase` only — injects CSS custom properties (`--theme-color-*`) for each `[data-theme]` selector
    - Converts `ColorTokens` keys from camelCase to kebab-case for CSS output
    - Does NOT register theme keys — handled by `@theme inline` in CSS (single responsibility)
5. [ ] Install `@fontsource/geist-mono` as dependency
6. [ ] Create `src/theme-provider.tsx`:
    - `<ThemeProvider>` context — passes `{ theme, setTheme, availableThemes }` to children
    - Reads initial theme from `localStorage`, defaults to `"obsidian"`
    - Writes `data-theme="..."` attribute on `<html>` element
    - Listens for `storage` events (syncs across tabs)
7. [ ] Update `src/styles.css`:
    - `@import "tailwindcss"`
    - `@import "@fontsource/geist-mono"`
    - `@plugin "./design/plugin.ts"` — injects CSS custom properties per theme
    - `@theme inline { --color-*: var(--theme-color-*) }` — maps to Tailwind utility names
8. [ ] Update `src/index.ts` barrel to export `ColorTokens`, `themes`, `availableThemes`, `ThemeConfig`, `ThemeKey`, `ThemeProvider`, `useTheme`
9. [ ] Write Vitest test: `ThemeProvider` sets `data-theme` on html, persists to localStorage

## Token Schema

```
tokens.ts — ColorTokens interface (22 color slots)
├── surface                  ← page background
│   ├── surfaceContent       ← text on surface
│   ├── surfaceAlt           ← card/section background
│   └── surfaceAltContent    ← text on surfaceAlt
├── primary                  ← brand accent color
│   ├── primaryContent       ← text on primary
│   ├── primaryHover         ← hover state
│   └── primaryHoverContent  ← text on primaryHover
├── info                     ← informational
│   └── infoContent          ← text on info
├── success                  ← confirmation/positive
│   └── successContent       ← text on success
├── warning                  ← caution/attention
│   └── warningContent       ← text on warning
├── danger                   ← destructive/error
│   ├── dangerContent        ← text on danger
│   ├── dangerHover          ← hover state
│   └── dangerHoverContent   ← text on dangerHover
├── highEmphasis             ← headlines/primary body
├── mediumEmphasis           ← secondary body
├── lowEmphasis              ← captions/placeholders
└── border                   ← dividers/input borders
```

## Acceptance Criteria

- [ ] `import "@gestalt/core/styles.css"` imports shared Tailwind theme with 4 themes
- [ ] `import { tokens, themes } from "@gestalt/core"` exposes token types and theme values
- [ ] `ThemeProvider` sets `data-theme` attribute, persists to localStorage
- [ ] All 4 themes switch colors via `data-theme` attribute (obsidian, matrix, pearl, vapor)
- [ ] `@theme inline` maps CSS custom properties to Tailwind utility classes (e.g., `bg-primary`, `text-surface-content`)
- [ ] Token changes in `themes.ts` automatically propagate to CSS via the plugin