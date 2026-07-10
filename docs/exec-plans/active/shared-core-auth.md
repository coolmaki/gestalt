# Plan: Shared Core Package — Auth + Navigation + i18n

**Created:** 2026-07-10
**Status:** 🔜 Not started
**Project:** Supercluster (cross-project)
**Driving Agent:** human
**Depends on:** Plan 2 (`shared-core-design-tokens`)

## Goal

Add authentication utilities (token storage, API client, auth signal, protected routes), a mobile-style stack navigator, and internationalization support to `@supercluster/core`.

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Auth state as SolidJS signal | `createAuthSignal()` returns reactive `{ user, isAuthenticated, login, logout }`. Components re-render on auth state change. |
| Token refresh via interceptor | API client auto-injects Authorization header. On 401, attempts refresh. On refresh fail, triggers logout. Transparent to app code. |
| Stack navigator as standalone utility | `createStackNavigator()` returns `{ push, pop, back }`. Integrates with `@solidjs/router`. Mobile UX patterns (swipe back). |
| i18n via `@solid-primitives/i18n` | Lightweight (~1KB), signal-based reactivity, lazy-loaded translations. |
| English as default locale | All apps ship with `en.ts`. Additional locales are per-app. |

## Steps

### Auth (src/auth/)

1. [ ] Create `src/auth/token-storage.ts` — `getAccessToken`, `getRefreshToken`, `setTokens`, `clearTokens` (stored in memory)
2. [ ] Create `src/auth/api-client.ts` — `createApiClient(baseUrl)` with auto-injected Authorization header, 401 → refresh → retry, refresh fail → logout
3. [ ] Create `src/auth/auth-signal.ts` — `createAuthSignal(client)` with `login(email, assertion)`, `logout()`, `isAuthenticated`
4. [ ] Create `src/auth/protected-route.tsx` — `<ProtectedRoute>` redirecting unauthenticated to `/login`

### Navigation (src/navigation/)

5. [ ] Create `src/navigation/stack.tsx`:
   - `createStackNavigator()` → `{ navigator, push, pop }`
   - Push/pop with screen transitions (slide-left/right)
   - Back gesture (touch-swipe from left edge)
   - Header with back button at depth > 1
6. [ ] Write Vitest tests for token-storage, auth-signal, api-client, protected-route
7. [ ] Write component test for stack navigator (push/pop)

### i18n (src/i18n/)

8. [ ] Install `@solid-primitives/i18n`
9. [ ] Create `src/i18n/index.ts` — `createI18nProvider`, `useI18n` hook, `setLocale`, lazy-load locales
10. [ ] Create `src/i18n/en.ts` — default English dictionary (common UI, auth, settings strings)
11. [ ] Write Vitest test: `useI18n().t("ok")` returns "OK", locale switch works

## Acceptance Criteria

### Auth
- [ ] `createAuthSignal().login(email, assertion)` completes Passport login flow
- [ ] API client auto-injects Authorization header
- [ ] API client auto-refreshes on 401, retries request
- [ ] On refresh fail, tokens cleared and `isAuthenticated` becomes `false`
- [ ] `<ProtectedRoute>` redirects unauthenticated users to `/login`

### Navigation
- [ ] `push("/settings")` adds screen to stack
- [ ] `pop()` removes top screen, transitions back
- [ ] Back button visible when stack depth > 1
- [ ] Swipe-to-go-back gesture works on mobile

### i18n
- [ ] `useI18n().t("ok")` returns correct string for current locale
- [ ] `setLocale("fr")` switches language, components re-render
- [ ] English dictionary covers all common UI strings