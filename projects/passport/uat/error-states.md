# Error States UAT

## API-Level Error Responses
All API errors are returned as RFC 7807 Problem Details with friendly messages in the UI.

| Scenario | Endpoint | Expected Status | Expected UI Message |
|----------|----------|----------------|---------------------|
| Email already registered | `/auth/register/complete` | 409 Conflict | Could not create your account. Please try again. |
| Challenge expired | `/auth/register/complete` | 400 Bad Request | Could not create your account. Please try again. |
| User not found | `/auth/login/begin` | 404 Not Found | Could not sign in. Please try again. |
| Wrong assertion | `/auth/login/complete` | 400 Bad Request | Could not sign in. Please try again. |
| Invalid email format | Various | 400 Bad Request | Depends on path |
| Missing required field | Various | 400 Bad Request | Depends on path |
| No credentials | `/auth/credentials?email=...` | 200 OK (empty list) | "No passkeys found." |
| Last credential removal | `DELETE /auth/credentials` | 409 Conflict | Could not update passkeys. Please try again. |
| Invalid recovery code | `/auth/recovery/verify-code` | 400 Bad Request | Recovery failed. Please try again. |
| Expired recovery token | `/auth/recovery/complete` | 400 Bad Request | Recovery failed. Please try again. |
| Invalid refresh token | `/auth/token/refresh` | 400/401 | Session error. Please sign in again. |

## WebAuthn Errors

| Scenario | Expected UI Message |
|----------|---------------------|
| Cancel passkey creation dialog | Could not create your passkey. Please try again. |
| Cancel passkey authentication dialog | Could not authenticate your passkey. Please try again. |
| Browser doesn't support passkeys | "This browser doesn't support passkeys..." (redirected to `/auth/unsupported`) |
| Hardware key error | (Browser handles; server sees attestation/assertion failure) |

## Session Errors

| Scenario | Expected Behavior |
|----------|------------------|
| Expired access token | Auto-refresh with refresh token (transparent to user) |
| Expired refresh token | Logged out, redirected to `/auth/login` with "Session expired" message |
| No tokens in storage | Direct access to `/dashboard` redirects to `/auth/login` |

## Network Errors

| Scenario | Expected Behavior |
|----------|------------------|
| Backend unreachable | fetch() throws, shown as generic error |
| CORS issues | Browser blocks; console shows CORS errors |

## Verification Code Errors

| Scenario | Expected UI Message |
|----------|---------------------|
| Wrong verification code | Verification failed. Please check your code and try again. |
| Expired code (>10 min) | Verification failed. Please check your code and try again. |
| Code already used | Verification failed. Please check your code and try again. |
