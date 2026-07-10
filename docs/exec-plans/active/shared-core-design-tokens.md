# Plan: Shared Core Package — Design Tokens

**Created:** 2026-07-10
**Status:** 🔜 Not started
**Project:** Supercluster (cross-project)
**Driving Agent:** human
**Depends on:** Plan 1 (`shared-core-scaffold`)

## Goal

Define the visual foundation of all Supercluster apps: design tokens, multi-theme CSS variable system, Geist Mono font, Tailwind v4 plugin for theme injection, and ThemeProvider. By the end of this plan, importing `@supercluster/core/styles.css` gives any app a consistent visual baseline.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| CSS custom properties for theming | `data-theme` attribute on `<html>` switches CSS variables. No JS needed for theme change — instant, no flash. |
| Token layer in TypeScript | `tokens.ts` is the single source of truth. A Tailwind v4 plugin reads token values and injects CSS custom properties via `addBase` + registers theme keys for utility class generation. Components reference tokens via Tailwind classes. One file to change, everything updates. |
| Light/dark built-in, named themes extensible | Tailwind's `dark:` prefix handles light/dark. Named themes (`terminal`, `high-contrast`) via `data-theme` are app-extensible. |
| Geist Mono via `@fontsource/geist-mono` | Variable weight font, open source, one package. Apps get the font through core's dependency. |
| Theme persist in localStorage | `ThemeProvider` writes to `localStorage` on change, reads on mount. No flash of wrong theme. |

## Steps

1. [ ] Create `web/@supercluster/core/src/design/` directory
2. [ ] Create `src/design/tokens.ts`:
   - Colors: `primary`, `primaryHover`, `surface`, `surfaceAlt`, `textPrimary`, `textSecondary`, `border`, `error`, `errorHover`
   - Each color has light + dark values (CSS var approach)
   - Spacing scale: `xs(4) through 3xl(32)` in px
   - Radii: `none(0)`, `sm(4)`, `md(8)`, `lg(12)`, `full(9999)`
   - Font family: `geistMono` (system-ui fallback)
   - Font sizes: `xs` through `3xl`
   - Shadows: `sm`, `md`, `lg`
3. [ ] Create `src/design/themes.ts`:
   - Default theme: `light` (maps to CSS vars without `data-theme`)
   - Built-in: `dark` (maps to `[data-theme="dark"]`)
   - Example second named theme: `terminal` (green-on-black aesthetic)
   - Export `ThemeConfig` type for type-safe theme definitions
4. [ ] Create `src/design/plugin.ts`:
    - Tailwind v4 plugin that imports `tokens.ts`
    - Uses `addBase` to inject CSS custom properties for `:root` (light), `[data-theme="dark"]`, and named themes like `[data-theme="terminal"]`
    - Registers theme keys (colors, spacing, radii, fonts, shadows) so Tailwind utility classes are generated from token values
5. [ ] Install `@fontsource/geist-mono` as dependency
6. [ ] Create `src/theme-provider.tsx`:
    - `<ThemeProvider>` context — passes `{ theme, setTheme, availableThemes }` to children
    - Reads initial theme from `localStorage`, defaults to `"light"`
    - Writes `data-theme="..."` attribute on `<html>` element
    - Listens for `storage` events (syncs across tabs)
7. [ ] Update `src/styles.css` — add `@import "tailwindcss"` and `@plugin "@supercluster/core/plugin"` to load the token plugin
8. [ ] Update `src/index.ts` barrel to export `tokens`, `plugin`, `ThemeProvider`, theme types
9. [ ] Write Vitest test: `ThemeProvider` sets `data-theme` on html, persists to localStorage

## Token Schema

```
tokens.ts
├── colors
│   ├── primary              ← main brand color
│   ├── primaryHover         ← hover states
│   ├── surface              ← page background
│   ├── surfaceAlt           ← card/section background
│   ├── textPrimary          ← headlines, body
│   ├── textSecondary        ← captions, metadata
│   ├── border               ← dividers, input borders
│   ├── error                ← validation messages
│   └── errorHover           ← error button hover
├── spacing                  ← 4px-32px scale
├── radii                    ← 0-9999px scale
├── fontFamily               ← "Geist Mono", monospace
├── fontSize                  ← xs-3xl
└── boxShadow                ← sm, md, lg
```

## Acceptance Criteria

- [ ] `import "@supercluster/core/styles.css"` imports shared Tailwind theme
- [ ] `import { tokens } from "@supercluster/core"` exposes token values for programmatic use
- [ ] `ThemeProvider` sets `data-theme` attribute, persists to localStorage
- [ ] `dark` theme switches colors via Tailwind's `dark:` prefix
- [ ] `terminal` theme applied via `data-theme="terminal"` with CSS vars
- [ ] Token changes propagate to Tailwind config automatically