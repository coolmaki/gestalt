# Gestalt Theme System — Design Document

**Created:** 2026-08-09
**Last updated:** 2026-08-09
**Status:** Final

This document defines the design contracts for the Gestalt theme system. Implementation plans reference this document; it does not change without a deliberate update.

---

## 1. Token Taxonomy

### 1.1 Category Assignment

| Category | Per-theme (user supplies) | Global (ships with `@gestalt/core`) |
|---|---|---|
| Colors | 69 tokens | — |
| Typography / font family | 1 token | — |
| Shadows | 5 tokens | — |
| Border width | 1 token | — |
| Type scale (sizes, weights, line-heights) | — | Yes |
| Spacing scale | — | Yes |
| Border radius | — | Yes (`data-radius` system, orthogonal) |
| Motion/duration | — | Yes |
| Z-index | — | Yes |
| Letter-spacing | — | Yes |

### 1.2 Colors (69 required)

Every **semantic color role** has exactly 5 interaction states, each with a surface/content pair. Structural colors have only a default value. Token names use camelCase.

**Semantic colors** — 6 roles × 5 states × 2 surface/content = 60 tokens:

| Role | Surface tokens | Content tokens |
|---|---|---|
| `primary` | default, `primaryHover`, `primaryActive`, `primaryFocus`, `primaryDisabled` | `primaryContent`, `primaryHoverContent`, `primaryActiveContent`, `primaryFocusContent`, `primaryDisabledContent` |
| `secondary` | default, `secondaryHover`, `secondaryActive`, `secondaryFocus`, `secondaryDisabled` | `secondaryContent`, `secondaryHoverContent`, `secondaryActiveContent`, `secondaryFocusContent`, `secondaryDisabledContent` |
| `info` | default, `infoHover`, `infoActive`, `infoFocus`, `infoDisabled` | `infoContent`, `infoHoverContent`, `infoActiveContent`, `infoFocusContent`, `infoDisabledContent` |
| `success` | default, `successHover`, `successActive`, `successFocus`, `successDisabled` | `successContent`, `successHoverContent`, `successActiveContent`, `successFocusContent`, `successDisabledContent` |
| `warning` | default, `warningHover`, `warningActive`, `warningFocus`, `warningDisabled` | `warningContent`, `warningHoverContent`, `warningActiveContent`, `warningFocusContent`, `warningDisabledContent` |
| `danger` | default, `dangerHover`, `dangerActive`, `dangerFocus`, `dangerDisabled` | `dangerContent`, `dangerHoverContent`, `dangerActiveContent`, `dangerFocusContent`, `dangerDisabledContent` |

Naming convention: `{role}` (default, no suffix), `{role}Hover`, `{role}Active`, `{role}Focus`, `{role}Disabled`. Content variants follow the same pattern with `Content` suffix.

**Structural colors** — 9 tokens, default only (no interaction states):

```
surface             ← page background
surfaceContent      ← text on surface
surfaceAlt          ← card / section / modal background
surfaceAltContent   ← text on surfaceAlt
highEmphasis        ← headlines, primary body text
mediumEmphasis      ← secondary body text, captions
lowEmphasis         ← placeholders, disabled hints
border              ← dividers, input borders
overlay             ← modal backdrop / scrim
```

### 1.3 Typography (1 required)

| Token | Validation | Values |
|---|---|---|
| `fontFamily` | Enum | `geist-mono`, `jetbrains-mono`, `fira-code`, `ibm-plex-mono`, `system-mono` |

All applications use monospace fonts exclusively. The `system-mono` option resolves to the browser/OS default monospace font.

**Bundled fonts** (SIL OFL 1.1, installed via `@fontsource`):
- Geist Mono (`@fontsource/geist-mono`) — already installed
- JetBrains Mono (`@fontsource/jetbrains-mono`)
- Fira Code (`@fontsource/fira-code`)
- IBM Plex Mono (`@fontsource/ibm-plex-mono`)

**System fonts** (referenced by name only, no distribution):
- `system-mono` → CSS `monospace` generic family

### 1.4 Shadows (5 required)

DTCG shadow objects with the shape `{ $type: "shadow", $value: { offsetX, offsetY, blur, spread, color } }`. All dimension sub-objects are `{ value: number, unit: "px" | "rem" }`.

| Token | Purpose |
|---|---|
| `shadowXs` | Subtle elevation (input inset, table row hover) |
| `shadowSm` | Card / light surface raise |
| `shadowMd` | Dropdown, popover, menu |
| `shadowLg` | Modal / dialog |
| `shadowXl` | Drawer / sheet |

### 1.5 Borders (1 required)

| Token | Type | Purpose |
|---|---|---|
| `borderWidth` | DTCG dimension `{ value, unit }` | Default border width |

Border color is sourced from `colors.border`. Border style defaults to `solid` globally — not user-configurable.

### 1.6 Total Token Count

| Category | Count |
|---|---|
| Semantic colors | 60 |
| Structural colors | 9 |
| Typography | 1 |
| Shadows | 5 |
| Borders | 1 |
| **Total** | **76** |

All 76 tokens are required. Missing any token = validation failure.

---

## 2. Schema Design

### 2.1 Location

```
web/@gestalt/core/schemas/
└── theme-schema.json
```

Single file, no versioning. The schema is the single source of truth for the theme contract. Owned by `@gestalt/core`.

### 2.2 Theme JSON Top-Level Shape

| Field | Required | Type | Constraints |
|---|---|---|---|
| `name` | Yes | string | max 64 chars |
| `description` | No | string | max 500 chars |
| `author` | No | string | max 64 chars |
| `colors` | Yes | object | 69 required keys |
| `typography` | Yes | object | 1 required key |
| `shadows` | Yes | object | 5 required keys |
| `borders` | Yes | object | 1 required key |

### 2.3 DTCG Value Shapes

Every token value is a DTCG-compliant `{ $type, $value }` object.

| `$type` | `$value` shape |
|---|---|
| `"color"` | Hex string `"#rrggbb"` or `"#rrggbbaa"` — OR — `{ colorSpace: "srgb"\|"display-p3"\|..., components: number[], alpha?: number }` |
| `"fontFamily"` | Array of font name strings (CSS font stack) |
| `"shadow"` | `{ offsetX: dimension, offsetY: dimension, blur: dimension, spread: dimension, color: color }` |
| `"dimension"` | `{ value: number, unit: "px" \| "rem" }` |

The schema uses `oneOf` to accept both hex and channel-based color formats. Hex is the primary recommended format.

### 2.4 Example Theme File

```json
{
  "name": "Custom Theme",
  "colors": {
    "surface": { "$type": "color", "$value": "#0a0a0a" },
    "primary": { "$type": "color", "$value": "#a78bfa" },
    "primaryHover": { "$type": "color", "$value": "#c4b5fd" },
    "primaryDisabled": { "$type": "color", "$value": "#4a3d6b" },
    "danger": { "$type": "color", "$value": "#fca5a5" },
    "highEmphasis": { "$type": "color", "$value": "#f0f0f0" },
    "border": { "$type": "color", "$value": "#1a1a1a" },
    "overlay": { "$type": "color", "$value": "#00000080" }
  },
  "typography": {
    "fontFamily": { "$type": "fontFamily", "$value": ["JetBrains Mono", "monospace"] }
  },
  "shadows": {
    "shadowSm": {
      "$type": "shadow",
      "$value": {
        "offsetX": { "value": 0, "unit": "px" },
        "offsetY": { "value": 1, "unit": "px" },
        "blur": { "value": 2, "unit": "px" },
        "spread": { "value": 0, "unit": "px" },
        "color": "#0000001a"
      }
    }
  },
  "borders": {
    "borderWidth": { "$type": "dimension", "$value": { "value": 1, "unit": "px" } }
  }
}
```

---

## 3. Primitive → Semantic Mapping

### 3.1 Architecture

```
┌──────────────────────────────────────────────┐
│  PRIMITIVES  (internal, TypeScript only)      │
│  web/@gestalt/core/src/design/primitives.ts   │
│                                                │
│  Raw palette, type scale, spacing scale        │
│  Used to AUTHOR built-in themes                │
│  NOT in JSON Schema — NOT user-facing          │
└──────────────────────┬───────────────────────┘
                       │ authoring only
                       ▼
┌──────────────────────────────────────────────┐
│  SEMANTIC TOKENS  (the public contract)        │
│  Defined by theme-schema.json                  │
│                                                │
│  76 tokens: 69 color + 1 font + 5 shadow +     │
│  1 border width                                 │
│                                                │
│  Built-in themes ship as resolved semantic     │
│  values. User themes provide them directly.    │
└──────────────────────┬───────────────────────┘
                       │ runtime
                       ▼
┌──────────────────────────────────────────────┐
│  CSS CUSTOM PROPERTIES                        │
│  --theme-color-surface                        │
│  --theme-color-primary-hover                  │
│  --theme-font-family                          │
│  --theme-shadow-md                            │
│  --theme-border-width                         │
│                                                │
│  Injected per [data-theme="..."]               │
└──────────────────────┬───────────────────────┘
                       │ Tailwind @theme mapping
                       ▼
┌──────────────────────────────────────────────┐
│  TAILWIND UTILITIES                           │
│  bg-surface, text-primary-content             │
│  hover:bg-primary-hover                       │
│  active:bg-primary-active                     │
│  focus-visible:ring-primary-focus             │
│  shadow-md, font-mono, etc.                   │
└──────────────────────────────────────────────┘
```

### 3.2 CSS Variable Naming Convention

```
{category}/{tokenKey} → --theme-{namespace}-{kebab-key}

colors.primary          → --theme-color-primary
colors.primaryHover     → --theme-color-primary-hover
colors.primaryActive    → --theme-color-primary-active
colors.primaryFocus     → --theme-color-primary-focus
colors.primaryDisabled  → --theme-color-primary-disabled
colors.surface          → --theme-color-surface
colors.highEmphasis     → --theme-color-high-emphasis
colors.overlay          → --theme-color-overlay
typography.fontFamily   → --theme-font-family
shadows.shadowMd        → --theme-shadow-md
borders.borderWidth     → --theme-border-width
```

These CSS variables are then consumed by Tailwind v4's `@theme` block:

```css
@theme {
  --color-primary: var(--theme-color-primary);
  --color-primary-hover: var(--theme-color-primary-hover);
  --color-primary-active: var(--theme-color-primary-active);
  --color-primary-focus: var(--theme-color-primary-focus);
  --color-primary-disabled: var(--theme-color-primary-disabled);
  /* ... all 69 color mappings ... */
  --font-family-mono: var(--theme-font-family);
  --shadow-xs: var(--theme-shadow-xs);
  --shadow-sm: var(--theme-shadow-sm);
  --shadow-md: var(--theme-shadow-md);
  --shadow-lg: var(--theme-shadow-lg);
  --shadow-xl: var(--theme-shadow-xl);
}
```

---

## 4. Validation

### 4.1 Validation Points

| Point | Location | Library | When |
|---|---|---|---|
| Upload time | Passport .NET backend | `JsonSchema.Net` | `POST /api/v1/personalization/themes` |
| Build time | `@gestalt/core` Vitest | `ajv` | `pnpm validate:themes` |
| Load time (optional) | Browser | `ajv` | Theme fetch from API |

### 4.2 Error Response Format (RFC 7807)

```json
{
  "type": "https://passport.gestalt.example.com/errors/theme-validation",
  "title": "Theme validation failed",
  "status": 422,
  "errors": [
    { "path": "/colors/primaryActive", "message": "Required token is missing." },
    { "path": "/shadows/shadowMd/$value/offsetY/unit", "message": "Expected 'px' or 'rem'." },
    { "path": "/typography/fontFamily", "message": "Must be one of: geist-mono, jetbrains-mono, fira-code, ibm-plex-mono, system-mono." }
  ]
}
```

### 4.3 Sanity Checks (non-blocking warnings)

Run after schema validation passes. Returned as `warnings[]` in the response body — do not cause rejection.

- Contrast ratio below WCAG AA threshold (surfaceContent vs surface, primaryContent vs primary, etc.)
- Duplicate hex values across semantic roles (e.g., `primary === danger`)

### 4.4 Backend Schema Access

The Passport backend reads `web/@gestalt/core/schemas/theme-schema.json` at startup via a configurable filesystem path (`ThemeValidationOptions.SchemaFilePath`). In CI, this file is included in the Docker image or published output alongside the backend assembly.

---

## 5. Consumption Path

### 5.1 End-to-End Flow

```
1. User authors theme JSON (manual or future UI)
2. POST /api/v1/personalization/themes → Passport validates → stores → returns ID
3. User selects theme in ThemeSettings page → PUT /api/v1/personalization/themes/preferences
4. Passport includes theme claim in JWT on next token refresh
5. On page load, blocking inline script reads JWT theme claim → sets data-theme attr
6a. Built-in: CSS already in stylesheet from build → done
6b. Custom: ThemeProvider fetches JSON → generates <style> → injects → done
```

### 5.2 Built-in vs. Custom CSS Injection

| Theme source | CSS generation | When |
|---|---|---|
| Built-in (Obsidian, etc.) | Tailwind plugin at build time (`plugin.ts`) | `@gestalt/core` build |
| Custom (user-uploaded) | Runtime JS in `ThemeProvider` | On theme load or switch |

Both produce identical CSS variable names under different `[data-theme="..."]` selectors. Tailwind's `@theme` mappings reference `var(--theme-*)` and are selector-agnostic.

### 5.3 FOUC Prevention

Blocking inline `<script>` in `index.html` (executes before any rendering):

1. Reads JWT theme claim (injected by server) or falls back to `localStorage`.
2. Sets `data-theme="obsidian"` (built-in) or `data-theme="custom-<uuid>"` immediately.
3. If custom, sets `window.__pendingCustomTheme = "<uuid>"` so ThemeProvider knows to fetch the CSS.

### 5.4 Dark/Light Mode

Not a schema dimension. A dark variant of a theme is a separate theme. Users switch between "My Theme" and "My Theme (Dark)" as distinct entries. This avoids the friction of requiring every theme author to provide both modes.

---

## 6. Ownership

| Concern | Owner |
|---|---|
| JSON Schema | `@gestalt/core` (`schemas/theme-schema.json`) |
| TypeScript type definitions | `@gestalt/core` (`src/design/tokens.ts`) |
| Primitive palette | `@gestalt/core` (`src/design/primitives.ts`) |
| Built-in theme values | `@gestalt/core` (`src/design/themes.ts`) |
| Tailwind plugin (CSS generation, build-time) | `@gestalt/core` (`src/design/plugin.ts`) |
| ThemeProvider + CSS runtime injection | `@gestalt/core` (`src/theme-provider.tsx`) |
| CSS-to-utility mapping | `@gestalt/core` (`src/styles.css`) |
| Theme storage (DB) | Passport backend |
| Theme validation endpoint | Passport backend |
| Theme management endpoints | Passport backend |
| JWT theme claim | Passport backend (auth flow) |
| Theme management UI page | Passport frontend |

---

## 7. Component Token Contract (v1 — Semantic Only)

Component-level tokens (e.g., `button-padding-sm`, `input-border-radius`) are derived from semantic tokens in CSS and are NOT part of the theme schema. This is a v1 scope decision. If component tokens are needed later, they will be added as an additive schema change in a follow-up plan.