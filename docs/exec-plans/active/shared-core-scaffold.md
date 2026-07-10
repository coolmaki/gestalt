# Plan: Shared Core Package — Scaffold

**Created:** 2026-07-10
**Status:** ✅ Completed
**Project:** Supercluster (cross-project)
**Driving Agent:** human
**Depends on:** Nothing

## Goal

Bootstrap the pnpm workspace and scaffold the `@supercluster/core` package — the single shared frontend platform for all Supercluster apps. By the end of this plan, `pnpm build` compiles an empty library successfully.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| pnpm workspaces | AGENTS.md mandates pnpm. Workspaces enable `web/*` packages to reference each other as `@supercluster/core` without npm link. |
| Vite library mode | Outputs ESM + TypeScript declarations. All consumers are Vite-based SolidJS apps — no CJS consumer exists. |
| Single package `@supercluster/core` | One version, one dependency per app. Scope: design + components + PWA + auth + nav + i18n. |
| `shamefully-hoist=true` | pnpm is strict by default. Many frontend deps expect flat `node_modules`. Hoist avoids cryptic build failures during component development. |
| `web/@supercluster/core/` path | Namespaced under `@supercluster/` for shared packages. Apps at top level: `web/passport/`. |
| ESM-only build | No legacy consumers exist. All apps use Vite. Eliminates dual-package hazard — SolidJS contexts and signals break with two module instances. |
| Tailwind v4 | Greenfield project. Vite-native `@tailwindcss/vite` replaces PostCSS chain. Faster builds, smaller install. CSS-first config via `@import "tailwindcss"` + `@theme`. |
| CSS file export strategy | Simplest path for Plan 1 (zero components). Library ships `src/styles.css` with `@import "tailwindcss"` + shared `@theme`. Graduate to Vite plugin later if needed. |
| `vite-plugin-dts` for declarations | Only viable way to emit `.d.ts` in Vite library mode. `tsc --emitDeclarationOnly` is slower and ignores Vite aliases. |

## Steps

1. [ ] Create `pnpm-workspace.yaml` at repo root:
   ```yaml
   packages:
     - 'web/*'
     - 'web/@supercluster/*'
   ```
2. [ ] Create root `package.json` (workspace root, `private: true`, defines `engines.pnpm`)
3. [ ] Create `.npmrc` with `shamefully-hoist=true`
4. [ ] Create `web/@supercluster/core/` directory structure:
   ```
   web/@supercluster/core/
   ├── src/
   │   ├── index.ts
   │   └── styles.css
   ├── package.json
   ├── tsconfig.json
   └── vite.config.ts
   ```
5. [ ] Write `package.json` — name `@supercluster/core`, type `module`, peer deps on `solid-js`, dev deps on `vite`, `typescript`, `@tailwindcss/vite`, `vite-plugin-solid`, `vite-plugin-dts`, `vitest`, `@solidjs/testing-library`
6. [ ] Write `tsconfig.json` — `module: ESNext`, `moduleResolution: bundler`, `jsx: preserve`, `jsxImportSource: solid-js`, strict, paths for `@supercluster/core` → `./src`
7. [ ] Write `vite.config.ts` — library mode, ESM-only, entry: `src/index.ts`, externalize `solid-js`, `vite-plugin-dts` for declarations
8. [ ] Write `src/styles.css` — `@import "tailwindcss"` with empty `@theme {}` block (replaced in Plan 2)
9. [ ] Write `src/index.ts` — barrel: `export {}` (empty), re-export CSS import
10. [ ] Run `pnpm install` from repo root
11. [ ] Run `pnpm build` from `web/@supercluster/core/` — verify zero errors, dist output exists

## Acceptance Criteria

- [ ] `pnpm-workspace.yaml` at repo root with `web/*` and `web/@supercluster/*`
- [ ] `.npmrc` with `shamefully-hoist=true`
- [ ] `web/@supercluster/core/package.json` with correct name and peer deps
- [ ] `pnpm install` succeeds
- [ ] `pnpm build` produces `dist/` with `.js` and `.d.ts` outputs
- [ ] `pnpm build` exits with zero errors