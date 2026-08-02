# Authentication UAT

## Prerequisites
- Registered and verified user account
- At least one passkey registered

## Happy Path

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Visit `/auth/login` | Centered card with "Sign in" form |
| 2 | Enter registered email | Email field populates |
| 3 | Click "Continue" | Browser passkey prompt appears |
| 4 | Complete passkey authentication (touch/scan) | Redirect to `/dashboard` |
| 5 | Verify dashboard shows passkey list | Passkey list displays with device info and creation date |

## Error States

| Scenario | Action | Expected Result |
|----------|--------|----------------|
| Empty email | Click "Continue" with blank email | Button disabled, cannot submit |
| Unregistered email | Enter email not in system | Browser passkey prompt still appears but API returns error: "Could not sign in." |
| Cancel passkey dialog | Cancel browser passkey prompt | Error: "Could not authenticate your passkey." |
| Unverified email | Login with unverified account email | Error from API. Friendly message: "Could not sign in." |
| No passkeys on this device | Login with email that has passkeys but none on this device | (Requires add-passkey flow — see add-passkey.md) |

## Navigation
| Link | Expected |
|------|----------|
| "Create an account" | Navigate to `/auth/register` |
| "Recover account" | Navigate to `/auth/recovery` |

## Notes
- After successful login, access token is stored in sessionStorage
- Refreshing the page should restore the session if tokens are still valid
- After token expiry, user should be redirected to login
