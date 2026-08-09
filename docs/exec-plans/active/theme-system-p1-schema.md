# Plan: Theme System — Phase 1: JSON Schema & Type Definitions

**Created:** 2026-08-09
**Status:** Active
**Project:** Gestalt (cross-project)
**Depends on:** `shared-core-design-tokens` (completed)
**Design doc:** `docs/design-docs/theme-system.md`

## Goal

Create the JSON Schema that defines the closed set of 76 semantic tokens every theme must implement, and the matching TypeScript type definitions. This establishes the contract all subsequent phases build against.

## Scope

- `theme-schema.json` — JSON Schema draft-2020-12, 76 required tokens with DTCG value shapes
- `tokens.ts` — TypeScript interfaces mirroring the schema (`ColorTokens`, `ShadowTokens`, `BorderTokens`, `TypographyTokens`, `ThemeTokens`)
- `primitives.ts` — internal palette / scale constants (not exported, not in schema)
- Schema validated with `ajv` in a Vitest test using a minimal valid fixture and several invalid fixtures

Out of scope: built-in theme values, plugin changes, ThemeProvider changes, component updates.

## Design Decisions

| Decision | Rationale |
|---|---|
| JSON Schema draft 2020-12 | Latest stable draft; `ajv` and `JsonSchema.Net` both support it |
| `$defs` for color shapes | Shared `colorState` and `semanticRole` definitions eliminate repetition across 60 semantic color slots |
| `oneOf` for hex vs channel colors | DTCG supports both; hex is primary, channels for Figma interop |
| `additionalProperties: false` | Strict mode — no extra keys accepted in any object |
| Schema as single file | No `$ref` to external files — backend reads one file, no resolution complexity |
| No `$id` URI | Not publishing the schema; internal-only validation |

## Steps

### 1. Create schema directory

Create `web/@gestalt/core/schemas/` directory.

### 2. Create `theme-schema.json`

JSON Schema draft-2020-12 with the following structure:

- **`$defs`** — reusable sub-schemas:
  - `hexColor`: `{ $type: "color", $value: string pattern "^#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?$" }`
  - `channelColor`: `{ $type: "color", $value: { colorSpace, components: number[], alpha? } }`
  - `colorToken`: `oneOf` over `hexColor` and `channelColor`
  - `dimensionToken`: `{ $type: "dimension", $value: { value: number, unit: enum("px","rem") } }`
  - `shadowToken`: full DTCG shadow shape with 5 dimension sub-objects + a `colorToken` for the shadow color
  - `fontFamilyToken`: `{ $type: "fontFamily", $value: array of strings }`
  - `semanticColorStates`: object generator for the 5 states × 2 content for one role
- **Top-level `properties`**:
  - `name` (string, max 64, required)
  - `description` (string, max 500, optional)
  - `author` (string, max 64, optional)
  - `colors` (object with 69 required properties using `$ref` to color sub-schemas)
  - `typography` (object with `fontFamily` using `$ref` + enum constraint)
  - `shadows` (object with 5 shadow levels)
  - `borders` (object with `borderWidth`)
- **`required`**: `["name", "colors", "typography", "shadows", "borders"]`
- **`additionalProperties: false`** at every level

The 60 semantic color keys must be explicitly enumerated. Do NOT use `patternProperties` — the schema must verify exact key names.

Font family uses `enum`: `["geist-mono", "jetbrains-mono", "fira-code", "ibm-plex-mono", "system-mono"]`.

### 3. Rewrite `tokens.ts`

Replace the current file at `web/@gestalt/core/src/design/tokens.ts`:

- `SemanticColorStateTokens`: 5 states (default, hover, active, focus, disabled) × 2 variants (surface, content) = 10 fields
- `SemanticColorTokens`: 6 roles (`primary`, `secondary`, `info`, `success`, `warning`, `danger`) each typed as `SemanticColorStateTokens`
- `StructuralColorTokens`: 9 fields (`surface`, `surfaceContent`, `surfaceAlt`, `surfaceAltContent`, `highEmphasis`, `mediumEmphasis`, `lowEmphasis`, `border`, `overlay`)
- `ColorTokens`: intersection of `SemanticColorTokens` and `StructuralColorTokens`
- `ShadowTokens`: 5 fields (`shadowXs`, `shadowSm`, `shadowMd`, `shadowLg`, `shadowXl`) — each is a DTCG shadow object type
- `BorderTokens`: `borderWidth` as DTCG dimension object type
- `TypographyTokens`: `fontFamily` as DTCG font family type
- `ThemeTokens`: `{ colors: ColorTokens; typography: TypographyTokens; shadows: ShadowTokens; borders: BorderTokens }`

Also define the DTCG value shape types:
- `HexColorValue`: `{ $type: "color"; $value: string }`
- `ChannelColorValue`: `{ $type: "color"; $value: { colorSpace: string; components: number[]; alpha?: number } }`
- `ColorValue`: `HexColorValue | ChannelColorValue`
- `DimensionValue`: `{ $type: "dimension"; $value: { value: number; unit: "px" | "rem" } }`
- `ShadowValue`: `{ $type: "shadow"; $value: { offsetX: DimensionValue["$value"]; offsetY: DimensionValue["$value"]; blur: DimensionValue["$value"]; spread: DimensionValue["$value"]; color: string } }`
- `FontFamilyValue`: `{ $type: "fontFamily"; $value: string[] }`

### 4. Create `primitives.ts`

Create `web/@gestalt/core/src/design/primitives.ts`. Internal-only constants for authoring built-in themes. Not exported from `@gestalt/core`.

- Color palette: neutral scale (slate/gray tones), semantic hue scales, accent scales
- Type scale: font sizes, line heights, font weights for a full typographic scale
- Spacing scale: standard rem-based spacing increments

These are raw values used to populate the semantic tokens in the built-in themes. They have no schema and are not user-facing.

### 5. Create schema validation test

Create `web/@gestalt/core/tests/theme-schema.test.ts`:

- Install `ajv` as devDependency (`pnpm add -D ajv` in `@gestalt/core`)
- Load `theme-schema.json` at test time
- Test: a minimal but complete 76-token theme passes validation
- Test: missing required token (e.g., `colors.primaryActive`) fails with correct error path
- Test: wrong `$type` (e.g., `$type: "dimension"` on a color) fails
- Test: invalid hex color (e.g., `"#GGG"`) fails
- Test: invalid font key (e.g., `"comic-sans"`) fails
- Test: invalid border width unit (e.g., `"em"`) fails
- Test: extra unknown property at top level fails (`additionalProperties: false`)
- Test: missing entire category object (e.g., no `shadows`) fails

### 6. Update public API exports

Add new type exports to `web/@gestalt/core/src/index.ts`:
- `SemanticColorTokens`, `StructuralColorTokens`, `ColorTokens`, `ShadowTokens`, `BorderTokens`, `TypographyTokens`, `ThemeTokens`
- DTCG value types: `HexColorValue`, `ChannelColorValue`, `ColorValue`, `DimensionValue`, `ShadowValue`, `FontFamilyValue`

Do NOT export anything from `primitives.ts`.

### 7. Verify

- `pnpm test` in `@gestalt/core` passes (new tests + existing tests)
- `pnpm build` in `@gestalt/core` succeeds (new types compile)
- Schema passes JSON Schema meta-validation (valid JSON Schema draft 2020-12)

## Acceptance Criteria

- [ ] `theme-schema.json` exists at `web/@gestalt/core/schemas/theme-schema.json`
- [ ] Schema is valid JSON Schema draft 2020-12
- [ ] Schema defines exactly 76 required tokens with DTCG value shapes
- [ ] Schema uses `$defs` to avoid repetition across semantic color roles
- [ ] Schema enforces font family enum (5 values)
- [ ] Schema rejects extra properties at every level
- [ ] `tokens.ts` exports all interfaces matching the schema
- [ ] `primitives.ts` exists with palette/scale constants (internal only)
- [ ] Vitest suite validates a complete valid theme and 6+ invalid cases
- [ ] `pnpm test` passes, `pnpm build` succeeds
- [ ] No changes to built-in theme values or plugin behavior (those come in Phase 2)