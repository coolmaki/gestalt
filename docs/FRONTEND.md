# FRONTEND.md

Frontend conventions for the Supercluster monorepo. All apps in `apps/` follow these rules.

---

## Technology Stack

- **Framework**: [SolidJS](https://www.solidjs.com/) — reactive UI library with no virtual DOM
- **Styling**: [Tailwind CSS](https://tailwindcss.com/) — utility-first CSS
- **Language**: TypeScript (strict mode)
- **Package manager**: pnpm
- **Build tool**: Vite
- **PWA**: Custom service worker via `apps/shared-ui/`

---

## Project Structure

```
apps/{app-name}/
├── src/
│   ├── components/        ← reusable UI components
│   ├── pages/             ← route-level page components
│   ├── layouts/           ← page layout wrappers
│   ├── hooks/             ← custom reactive hooks (createResource, createEffect wrappers)
│   ├── signals/           ← global reactive state (createSignal, createStore)
│   ├── api/               ← typed fetch wrappers, API client
│   └── index.tsx          ← app entry point, router setup
├── public/                ← static assets, PWA manifest, icons
├── tailwind.config.ts     ← extends shared config from shared-ui
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
import { Result } from "shared-ui/results"; // or Supercluster primitives mirror
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

## Shared UI Package (`apps/shared-ui/`)

All design tokens and shared components live here:

```
apps/shared-ui/
├── src/
│   ├── design/
│   │   ├── tokens.ts       ← colors, fonts, spacing, breakpoints
│   │   └── tailwind.ts     ← shared Tailwind preset
│   ├── components/         ← shared components (Button, Input, Modal, etc.)
│   ├── navigation/         ← custom stack navigator
│   ├── pwa/                ← service worker registration, manifest helpers
│   └── index.ts            ← public API barrel export
├── tailwind.config.ts
└── package.json
```

### Using Shared UI

Apps extend the shared Tailwind config:

```ts
// apps/passport/tailwind.config.ts
import sharedPreset from "shared-ui/design/tailwind";

export default {
  presets: [sharedPreset],
  content: ["./src/**/*.{ts,tsx}", "../shared-ui/src/**/*.{ts,tsx}"],
};
```

---

## Styling Rules

1. **Tailwind-first.** No inline styles, no CSS modules. Utility classes only.
2. **Design tokens from `shared-ui`.** Colors, spacing, typography are defined once.
3. **Responsive by default.** Use `sm:`, `md:`, `lg:` breakpoints.
4. **Dark mode.** All apps support dark mode via Tailwind's `dark:` variant. Toggle via a signal in `shared-ui`.

---

## PWA Requirements

All frontend apps are PWAs. Shared PWA utilities in `apps/shared-ui/pwa/` provide:

- Service worker registration with update prompts
- Offline fallback page
- Install prompt handling
- Manifest generation helpers
