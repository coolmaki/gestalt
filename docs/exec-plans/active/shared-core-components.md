# Plan: Shared Core Package — Components + Storybook + MCP

**Created:** 2026-07-10
**Status:** 🔜 Not started
**Project:** Supercluster (cross-project)
**Driving Agent:** human
**Depends on:** Plan 2 (`shared-core-design-tokens`)

## Goal

Build the initial component library with Storybook 8 + MCP integration. Deliver 8 components (Icon, Button, Input, Text, Stack, Card, FormField, Modal) with compound variants, full test coverage, and agent-verifiable Storybook catalog.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Design Tokens + Compound Variants (Hybrid) | Tokens control global visual consistency. Variants provide type-safe component-level choices. |
| One component per directory | `src/components/Button/Button.tsx` + stories + tests co-located. Barrel exports at `components/index.ts`. |
| Icon via Lucide wrapper | `<Icon name="check" />` — name is our own labeled union, not Lucide's. Swap icon set by changing one mapping file. |
| Storybook 8 | Native SolidJS support via `storybook-solidjs`. |
| MCP server for Storybook | Agents read story metadata, open stories, and verify visual output via MCP protocol. |
| Vitest + @solidjs/testing-library | Render components, click handlers, assert accessibility states. |
| Mobile-first defaults | Designed for mobile viewport (375px). Desktop works but not prioritized. |

## Component Spec

### Icon
- Props: `name: IconName enum, size: number, class?: string`
- Internal mapping: `{ "check": () => <LucideCheck />, ... }`
- Not a re-export — own API, own test
- Stories: gallery of all icons

### Button
- Variants: `primary`, `secondary`, `ghost`
- Sizes: `sm`, `md`, `lg`
- States: enabled, disabled, loading (shows spinner), active
- Props: `variant, size, type, disabled, loading, onClick, children`

### Input
- States: default, focused, disabled, errored
- Props: `type, placeholder, value?, error?, disabled, onChange`

### Text
- Variants: `headline`, `subhead`, `body`, `caption`
- Props: `variant, as` (override HTML tag), `children`

### Stack
- Direction: `col` (default), `row`
- Gap: `xs | sm | md | lg | xl | 2xl | 3xl`
- Props: `direction, gap, align, justify, children`

### Card
- Props: `variant` (`default | ghost`), `children`

### FormField
- Composes: `<label>` + `children` (input slot) + error message
- Props: `label, error, htmlFor, children`

### Modal
- Props: `open, onClose, title, children`
- Fixed overlay, centered card, closes on backdrop + Escape, focus trap

## Steps

1. [ ] Create component directory structure (8 directories)
2. [ ] Implement Icon wrapper + Lucide icon mapping
3. [ ] Implement Button with compound variants
4. [ ] Implement Input with states
5. [ ] Implement Text with variants
6. [ ] Implement Stack with direction/gap
7. [ ] Implement Card
8. [ ] Implement FormField (label + slot + error)
9. [ ] Implement Modal (overlay + focus trap)
10. [ ] Set up Storybook 8 (`storybook-solidjs`, `.storybook/main.ts`, `.storybook/preview.ts`)
11. [ ] Write stories for all 8 components
12. [ ] Configure Storybook MCP server
13. [ ] Write Vitest tests for all 8 components (render, events, accessibility)
14. [ ] Export from barrel

## Acceptance Criteria

- [ ] All 8 components render in Storybook
- [ ] Storybook accessible at `pnpm storybook` (localhost:6006)
- [ ] MCP server allows agents to list stories and read component metadata
- [ ] All Vitest component tests pass
- [ ] Components themed via tokens — changing `tokens.ts` changes component appearance
- [ ] No Lucide type exposed in public API (only our `IconName` union)
- [ ] Components follow mobile-first: usable at 375px viewport