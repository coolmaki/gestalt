# Account Recovery UAT

## Prerequisites
- Registered and verified user account with at least one passkey
- No passkey available (simulate lost device)

## Happy Path (4-step wizard)

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Visit `/auth/recovery` | Centered card with "Recover account" form |
| 2 | Enter registered email | Email field populates |
| 3 | Click "Send recovery code" | Message: "If this email is registered, we sent a recovery code." |
| 4 | Check terminal for recovery code | 6-digit recovery code is logged |
| 5 | Enter the 6-digit code | Transition to step 3: "Create a new passkey to recover your account." |
| 6 | Click "Create new passkey" | Browser passkey prompt appears |
| 7 | Complete passkey creation | Transition to step 4: "Account recovered! Redirecting..." |
| 8 | Wait for redirect | Redirected to `/auth/login` |
| 9 | Sign in with newly created passkey | Redirected to `/dashboard` with single passkey |

## Recovery Behavior
- All **existing passkeys are removed** during recovery
- The only passkey after recovery is the one just created
- This prevents the lost device's credentials from being a lingering security risk

## Error States

| Scenario | Action | Expected Result |
|----------|--------|----------------|
| Unregistered email | Begin recovery with unknown email | Same "If this email is registered..." message (no user enumeration) |
| Wrong recovery code | Enter incorrect 6-digit code | Error: "Invalid code" or "Recovery failed." |
| Expired recovery code | Wait 10+ minutes, then enter code | Error about invalid/expired code |
| Cancel passkey creation | Cancel browser passkey prompt | Error: "Could not create your passkey." |

## Notes
- Recovery silently succeeds for unknown emails — no user enumeration
- Recovery code is 6 digits, 10-minute TTL
- Max 3 code attempts (enforced server-side — not yet in UI)
- All existing passkeys are removed before the new one is added
