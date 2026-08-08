# Plan: Shared Core Package — Components

**Created:** 2026-07-10
**Status:** ✅ Completed
**Project:** Gestalt (cross-project)
**Driving Agent:** human
**Depends on:** Plan 2 (`shared-core-design-tokens`)

## Goal

Build the initial component library. Deliver 7 components (Icon, Button, Input, Text, Card, FormField, Modal) with compound variants, full test coverage, and themed via `@gestalt/core` design tokens.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Design Tokens + Compound Variants (Hybrid) | Tokens control global visual consistency. Variants provide type-safe component-level choices. |
| One component per directory | `src/components/Button/Button.tsx` + stories + tests co-located. Barrel exports at `components/index.ts`. |
| Icon via Lucide wrapper | `<Icon name="check" />` — name is our own labeled union, not Lucide's. Pull icon names as needed; no bulk import. Swap icon set by changing one mapping file. |
| Storybook 8 | Native SolidJS support via `storybook-solidjs` for visual dev. Storybook MCP deferred (React-only). |
| solid-focus-trap for Modal | 1.1 KB gzipped, native SolidJS reactive API, handles Tab cycling, focus restore, and DOM observation. Upgrade path to `corvu` if full Dialog needed. |
| Vitest + @solidjs/testing-library | Render components, click handlers, assert accessibility states. |
| Frontend-design agent skill | `.agents/skills/frontend-design/SKILL.md` guides agent-driven component creation and visual decisions. |
| Storybook deferred | `storybook-solidjs` ecosystem lacks stable version alignment with core Storybook. Deferred to backlog; rely on frontend-design skill and vitest for component dev. |

## Component Spec

### Icon
- Props: `name: IconName enum, size: number, class?: string`
- Internal mapping: `{ "check": () => <LucideCheck />, ... }`
- Not a re-export — own API, own test
- Stories: gallery of all icons

### Button
- Variants: `primary`, `secondary`, `ghost`, `danger`
- Sizes: `sm`, `md`, `lg`
- States: enabled, disabled, loading (shows spinner), active
- Props: `variant, size, type, disabled, loading, onClick, children`

### Input
- States: default, focused, disabled, errored
- Props: `type, placeholder, value?, error?, disabled, onChange`

### Text
- Variants: `headline`, `subhead`, `body`, `caption`
- Props: `variant, as` (override HTML tag), `children`

### Card
- Props: `variant` (`default | ghost`), `children`

### FormField
- Composes: `<label>` + `children` (input slot) + error message
- Props: `label, error, htmlFor, children`

### Modal
- Props: `open, onClose, title, children`
- Focus trap via `solid-focus-trap` (1.1 KB) — Tab cycling, focus restore, DOM observation
- Fixed overlay, centered card, closes on backdrop + Escape
- SDependency: `solid-focus-trap` added to `@gestalt/core`

## Steps

1. [ ] Create `.agents/skills/frontend-design/SKILL.md` from Anthropic skill repo (guides agent-driven component design)
2. [ ] Install `solid-focus-trap` dependency
3. [ ] Create component directory structure (7 directories)
4. [ ] Implement Icon wrapper + Lucide icon mapping (pull names as needed)
5. [ ] Implement Button with compound variants (primary, secondary, ghost, danger)
6. [ ] Implement Input with states
7. [ ] Implement Text with variants
8. [ ] Implement Card
9. [ ] Implement FormField (label + slot + error)
10. [ ] Implement Modal (overlay + focus trap via solid-focus-trap)
11. [ ] Write Vitest tests for all 7 components (render, events, accessibility)
12. [ ] Export from barrel

## Acceptance Criteria

- [ ] All 7 components render correctly
- [ ] All Vitest component tests pass
- [ ] Components themed via tokens — changing `tokens.ts` changes component appearance
- [ ] No Lucide type exposed in public API (only our `IconName` union)
- [ ] Modal focus trap works (Tab cycles within modal, focus restored on close)