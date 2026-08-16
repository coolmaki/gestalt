# Game Icon Pack

**Source:** [github.com/Nieobie/game-icon-pack](https://github.com/Nieobie/game-icon-pack) ([nieobie.itch.io/free-icons](https://nieobie.itch.io/free-icons))
**License:** [CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/) — free to use, modify, share, and distribute for any purpose.
**Stats:** 800+ icons, ~626★, actively updated via GitHub Actions CI/CD.

Rounded, monochrome, `fill="currentColor"` SVG icons. Two render variants ship with the pack:

- **`padding/`** — every icon uses a fixed `viewBox="0 0 10 10"` with built-in whitespace. This is the variant Gestalt vendors.
- **`no-padding/`** — each icon uses a tight per-icon bounding-box `viewBox`.

## How Gestalt consumes it

The SVGs are vendored into `web/@gestalt/core/scripts/icon-pack-src/` (all 13 category folders + `Icon_Catalog.json` + `LICENSE`). The generator `scripts/generate-icons.ts` parses each SVG, extracts the inner shape (`<path>` / `<rect>` / `<ellipse>`), and emits `src/components/Icon/icon-data.ts`:

```ts
export type IconShape =
  | { type: "path"; d: string }
  | { type: "rect"; x: number; y: number; width: number; height: number; rx?: number }
  | { type: "ellipse"; cx: number; cy: number; rx: number; ry: number };

export const ICON_VIEWBOX = "0 0 10 10";   // constant — shared by all icons

export const icons: Record<IconName, IconShape[]> = { ... };
export type IconName = (typeof iconNames)[number];
```

The `Icon` component renders each shape inside a single `<svg viewBox="0 0 10 10" fill="currentColor">`. Because all shapes share one viewBox, the renderer is trivial and the icons are optically consistent.

### Updating the icons

1. Re-download / re-sync the vendored SVGs into `scripts/icon-pack-src/` (either manually or by pulling from the upstream repo).
2. Run `pnpm generate-icons` from `web/@gestalt/core`.
3. Review the diff on `icon-data.ts`.

## `Icon_Catalog.json`

The upstream repo ships a per-icon metadata catalog (~170 KB, list of entries):

| Field | Description |
|---|---|
| `component_name` | Icon key, matches the SVG filename (e.g. `action-points`) |
| `visual_features` | Short visual description |
| `core_semantic` | Abstract meaning the icon conveys |
| `use_cases` | Suggested contexts (UI buttons, resource display, etc.) |
| `synonyms` | Search terms for matching (`close`, `cancel`, `cross`) |

Note: some field values are currently written in Chinese (the author writes both English and Chinese docs); treat the catalog as a starting point, not a polished localization.

### Possible uses

- **Fuzzy icon picker** — match a user query (`"close"`, `"cancel"`) against `synonyms` + `core_semantic` to suggest `cross`.
- **Human-readable labels** — derive display names for the dev gallery from `core_semantic`/`use_cases`.
- **LLM tooling** — the catalog is explicitly designed for programmatic retrieval and LLM batch processing.

None of these are wired up yet; the catalog is vendored as source-of-truth for future work.
