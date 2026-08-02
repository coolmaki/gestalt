# Dashboard UAT

## Prerequisites
- Logged in user with at least 2 passkeys registered

## Happy Path

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Visit `/dashboard` | Header bar with "Passport" title, user email, and "Log out" button |
| 2 | Verify passkey list | All registered passkeys displayed with device name and creation date |
| 3 | Verify each passkey row | Icon + device name + "Created {date}" |
| 4 | Click "Log out" button | Redirected to `/auth/login`, tokens cleared |

## Remove Passkey

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Click trash icon on a passkey | Modal opens: "Remove passkey" with confirmation |
| 2 | Click "Cancel" in modal | Modal closes, passkey still listed |
| 3 | Click trash icon, then "Remove" | Passkey removed from list, modal closes |
| 4 | Remove last passkey | Error from API. Friendly message: "Could not update passkeys." |

## Log Out Everywhere

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Click "Log out everywhere" button | All refresh tokens revoked |
| 2 | Verify redirect | Redirected to `/auth/login` |
| 3 | Verify session | Cannot refresh token — full re-login required |

## Session Persistence

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Log in and visit dashboard | Session active |
| 2 | Refresh the page | Dashboard reloads, still authenticated |
| 3 | Close tab, open new tab, visit `/dashboard` | Redirected to login (sessionStorage cleared) |

## Notes
- Passkey list is sorted by creation date
- Device name shows "Unnamed device" if none was set during creation
- Removing the last passkey is blocked server-side — user must use recovery instead
