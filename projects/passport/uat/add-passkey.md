# Add Passkey from New Device UAT

## Prerequisites
- Registered and verified user account
- At least one passkey on another device
- Current device has no passkey for this account
- Verified email delivery (check terminal output for verification codes in debug mode)

## Flow: Login with no credentials triggers add-passkey

The login flow should detect that no passkey exists on this device and guide the user through adding one. This flow is still in development — the Login page currently just shows an error.
(Manual testing of the add-passkey flow requires direct API calls for now.)

## Happy Path (4-step wizard)

| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Call `POST /api/v1/auth/credentials/add/begin` with email | 200 OK (silent) |
| 2 | Check terminal for device verification code | 6-digit code is logged |
| 3 | Call `POST /api/v1/auth/credentials/add/verify` with email + code | Returns `addPasskeyToken` |
| 4 | Call `POST /api/v1/auth/credentials/add/begin-registration` with token | Returns `optionsJson` |
| 5 | Call `POST /api/v1/auth/credentials/add/complete` with token + attestation | 200 OK |
| 6 | Call `GET /api/v1/auth/credentials?email=...` | Credential list now includes the new passkey |

## Passkey Preservation
- Existing passkeys from other devices should **remain intact**
- Unlike recovery, this flow does NOT remove existing passkeys
- Verify by listing credentials — count should increment by 1

## Error States

| Scenario | Action | Expected Result |
|----------|--------|----------------|
| Unregistered email | Begin add with unknown email | 200 OK (silent — no user enumeration) |
| Wrong code | Verify with incorrect code | Error: "Invalid verification code" |
| Expired code | Wait 10+ minutes, then verify | Error about expired code |
| Invalid token | Complete with invalid token | Error: "Invalid or expired add-passkey token" |
| Expired challenge | Wait 5+ minutes between begin-reg and complete | Error: "Registration challenge expired" |

## Notes
- Uses `RecoveryCodePurpose.DeviceVerification` code type
- Challenge store prefix: `add-passkey:{token}` (separate from recovery)
- Token TTL: 5 minutes
- Code TTL: 10 minutes
