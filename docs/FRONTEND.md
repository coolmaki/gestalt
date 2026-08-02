# FRONTEND.md

Frontend conventions for the Supercluster monorepo. All apps in `web/` follow these rules.

---

## Technology Stack

- **Framework**: [SolidJS](https://www.solidjs.com/) — reactive UI library with no virtual DOM
- **Styling**: [Tailwind CSS](https://tailwindcss.com/) — utility-first CSS
- **Language**: TypeScript (strict mode)
- **Package manager**: pnpm
- **Build tool**: Vite
- **PWA**: Custom service worker via `web/@supercluster/core/`

---

## Project Structure

```
web/{app-name}/
├── src/
│   ├── components/        ← reusable UI components
│   ├── pages/             ← route-level page components
│   ├── layouts/           ← page layout wrappers
│   ├── hooks/             ← custom reactive hooks (createResource, createEffect wrappers)
│   ├── signals/           ← global reactive state (createSignal, createStore)
│   ├── api/               ← typed fetch wrappers, API client
│   ├── index.tsx          ← app entry point, router setup
│   └── styles.css         ← local Tailwind entry (imports @supercluster/core styles, adds app-specific @theme overrides)
├── public/                ← static assets, PWA manifest, icons
├── vite.config.ts
├── tsconfig.json
└── package.json
```

---

## Component Conventions

### One Component Per File

```tsx
// src/components/UserCard.tsx
import { Component, Show } from "solid-js";

interface UserCardProps {
  name: string;
  email: string;
  avatarUrl?: string;
}

export const UserCard: Component<UserCardProps> = (props) => {
  return (
    <div class="rounded-lg border p-4 shadow-sm">
      <Show when={props.avatarUrl}>
        <img src={props.avatarUrl!} alt={props.name} class="h-10 w-10 rounded-full" />
      </Show>
      <h3 class="text-lg font-semibold">{props.name}</h3>
      <p class="text-sm text-gray-600">{props.email}</p>
    </div>
  );
};
```

### Rules
- Export components as named exports (not default).
- Props interface name: `{ComponentName}Props`.
- Use `Show`, `For`, `Switch`/`Match` control flow components — no manual ternaries in JSX.
- Tailwind classes directly on elements; no CSS modules or styled-components.
- Extract repeated class patterns into Tailwind `@apply` directives in `index.css` only when reused across many components.

---

## State Management

### Local State → `createSignal`

```tsx
const [count, setCount] = createSignal(0);
```

### Shared State → Signals Store (in `signals/`)

```tsx
// src/signals/auth.ts
import { createSignal } from "solid-js";

export const [currentUser, setCurrentUser] = createSignal<User | null>(null);
export const [isAuthenticated, setIsAuthenticated] = createSignal(false);
```

Keep signals co-located near where they're used. Global signals live in `src/signals/`.

---

## API Layer (`api/`)

All network calls go through a typed API client. No raw `fetch` in components.

```tsx
// src/api/users.ts
import { Result } from "@supercluster/core";
import { apiClient } from "./client";

export interface UserDto {
  id: string;
  name: string;
  email: string;
}

export async function getUser(id: string): Promise<Result<UserDto>> {
  return apiClient.get<UserDto>(`/users/${id}`);
}
```

The `apiClient` handles:
- Base URL resolution
- Auth token injection
- Error normalization to `Result<T>` (mirroring the backend pattern)
- Request/response logging in development

---

## Routing

Use SolidJS Router (`@solidjs/router`). Define routes in `src/index.tsx`:

```tsx
import { Router, Route } from "@solidjs/router";
import { MainLayout } from "./layouts/MainLayout";

export const App = () => (
  <Router>
    <Route path="/" component={MainLayout}>
      <Route path="/" component={lazy(() => import("./pages/Home"))} />
      <Route path="/login" component={lazy(() => import("./pages/Login"))} />
      <Route path="/dashboard" component={lazy(() => import("./pages/Dashboard"))} />
    </Route>
  </Router>
);
```

- Use `lazy` for route-level code splitting.
- One layout per route group.

---

## Testing

- **Unit tests**: Vitest for hooks, signals, and utility functions.
- **Component tests**: Solid Testing Library (`@solidjs/testing-library`).
- **E2E**: Playwright (future).

---

## Shared UI Package (`web/@supercluster/core/`)

All design tokens, components, PWA utilities, auth, navigation, and i18n live here in a single package:

```
web/@supercluster/core/
├── src/
│   ├── design/
│   │   ├── tokens.ts       ← colors, fonts, spacing, breakpoints
│   │   ├── themes.ts       ← named themes, CSS variable mappings
│   │   └── plugin.ts       ← Tailwind v4 plugin (reads tokens, injects CSS)
│   ├── components/         ← shared components (Button, Input, Modal, etc.)
│   ├── navigation/         ← custom stack navigator
│   ├── pwa/                ← service worker registration, manifest helpers
│   ├── auth/               ← token storage, API client, auth signal
│   ├── i18n/               ← internationalization utilities
│   ├── index.ts            ← public API barrel export
│   └── styles.css          ← shared Tailwind v4 entry (@import "tailwindcss" + @plugin)
└── package.json
```

### Using @supercluster/core

Apps import the shared CSS and can extend with app-specific theme overrides:

```css
/* web/passport/src/styles.css */
@import "@supercluster/core/styles.css";

@theme {
  /* override or extend shared tokens here */
}
```

```tsx
// web/passport/src/index.tsx
import "./styles.css";
// ...
```

---

## Styling Rules

1. **Tailwind-first.** No inline styles, no CSS modules. Utility classes only.
2. **Design tokens from `@supercluster/core`.** Colors, spacing, typography are defined once.
3. **Responsive by default.** Use `sm:`, `md:`, `lg:` breakpoints.
4. **Dark mode.** All apps support dark mode via Tailwind's `dark:` variant. Toggle via ThemeProvider in `@supercluster/core`.

---

## SPA Serving Pattern

When a .NET project also serves its SPA, follow this pattern to handle both development and production modes:

### Development (Debug + Development environment)

The SPA is served via `Microsoft.AspNetCore.SpaServices.Extensions`, which proxies non-API requests to the Vite dev server. This enables HMR and fast iteration.

**Configuration:**
- `launchSettings.json` sets `SpaProxyServerUrl` to `http://localhost:5173` and `ASPNETCORE_ENVIRONMENT` to `Development`
- `Program.cs` conditionally registers `UseSpa` only in Debug builds when the environment is `Development`
- The developer starts the Vite dev server separately with `pnpm --filter {app} dev`

**Startup:**
```bash
# Terminal 1 — SPA dev server
pnpm --filter passport dev

# Terminal 2 — .NET app (proxies SPA to Vite, handles API directly)
dotnet run --project src/Passport
```

### Production (Release)

The SPA is pre-built and served as static files from `wwwroot/`. A conditional MSBuild target in the `.csproj` runs `pnpm build` automatically in Release configuration.

**Startup (single command):**
```bash
dotnet run --project src/Passport -c Release
```

### Tests

Integration tests use the `"Test"` environment and run in Debug mode. The `Program.cs` guards (`#if DEBUG` + `IsDevelopment()`) prevent the SpaProxy from being registered, so tests exercise only API endpoints. The `test` appsettings file provides explicit SQLite configuration for the test database context.

### Required Conditions

| Mode | Build Config | Environment | SPA Source |
|------|-------------|-------------|------------|
| Development | Debug | Development | Vite dev server (via proxy) |
| Test | Debug | Test | None (API only) |
| Production | Release | Production | `wwwroot/` (pre-built) |

The `SpaProxyServerUrl` setting is required in Development mode and provided via `Properties/launchSettings.json`. It is absent in other environments, so the proxy is never activated unexpectedly.

All frontend apps are PWAs. Shared PWA utilities in `web/@supercluster/core/pwa/` provide:

- Service worker registration with update prompts
- Offline fallback page
- Install prompt handling
- Manifest generation helpers
