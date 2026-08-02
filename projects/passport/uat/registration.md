# Registration UAT

## Prerequisites
- Clean database (no existing accounts)
- Verified email delivery (check terminal output for verification codes in debug mode)

## Happy Path

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Visit `/auth/register` | Centered card with "Create account" form |
| 2 | Enter a valid email | Email field populates |
| 3 | Click "Create account" | Browser passkey prompt appears |
| 4 | Complete passkey creation (touch/scan) | Redirect to `/auth/verify?email=...` |
| 5 | Check terminal for verification code | 6-digit code is logged |
| 6 | Enter the 6-digit code | "Email verified! Redirecting to sign in..." |
| 7 | Wait for redirect | Redirected to `/auth/login` |
| 8 | Sign in with the new passkey | Redirected to `/dashboard` |

## Error States

| Scenario | Action | Expected Result |
|----------|--------|----------------|
| Empty email | Click "Create account" with blank email | Button disabled, cannot submit |
| Invalid email format | Enter "notanemail", click create | API returns validation error, friendly message shown |
| Duplicate email | Register again with same email | Error: "Could not create your account. Please try again." |
| Wrong verification code | Enter wrong 6-digit code | Error: "Verification failed. Please check your code and try again." |
| Expired verification code | Wait 10+ minutes, then enter code | Error: "Verification failed. Please check your code and try again." |
| Code already used | Enter same code twice | Second attempt: "Verification failed..." |
| Cancel passkey dialog | Cancel browser passkey prompt | Error: "Could not create your passkey." |

## Notes
- Registration creates the user account but email is unverified until code is entered
- Unverified users cannot authenticate
- The login link at the bottom navigates to `/auth/login`
