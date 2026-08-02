import { api } from "./client";
import type {
  BeginRegistrationResponse,
  BeginLoginResponse,
  SessionResult,
  VerifyRecoveryCodeResponse,
  VerifyAddPasskeyCodeResponse,
  GetCredentialsResponse,
  CredentialInfo,
} from "./types";

// --- Registration ---

export function beginRegistration(email: string): Promise<BeginRegistrationResponse> {
  return api.post<BeginRegistrationResponse>("/api/v1/auth/register/begin", { email });
}

export function completeRegistration(email: string, attestationJson: string): Promise<void> {
  return api.post<void>("/api/v1/auth/register/complete", { email, attestationJson });
}

export function verifyEmail(email: string, code: string): Promise<void> {
  return api.post<void>("/api/v1/auth/register/verify-email", { email, code });
}

// --- Login ---

export function beginLogin(email: string): Promise<BeginLoginResponse> {
  return api.post<BeginLoginResponse>("/api/v1/auth/login/begin", { email });
}

export function completeLogin(email: string, assertionJson: string): Promise<SessionResult> {
  return api.post<SessionResult>("/api/v1/auth/login/complete", { email, assertionJson });
}

// --- Recovery ---

export function beginRecovery(email: string): Promise<void> {
  return api.post<void>("/api/v1/auth/recovery/begin", { email });
}

export function verifyRecoveryCode(email: string, code: string): Promise<VerifyRecoveryCodeResponse> {
  return api.post<VerifyRecoveryCodeResponse>("/api/v1/auth/recovery/verify-code", { email, code });
}

export function beginRecoveryRegistration(recoveryToken: string): Promise<BeginRegistrationResponse> {
  return api.post<BeginRegistrationResponse>("/api/v1/auth/recovery/begin-registration", { recoveryToken });
}

export function completeRecovery(recoveryToken: string, attestationJson: string): Promise<void> {
  return api.post<void>("/api/v1/auth/recovery/complete", { recoveryToken, attestationJson });
}

// --- Add Passkey from New Device ---

export function beginAddPasskey(email: string): Promise<void> {
  return api.post<void>("/api/v1/auth/credentials/add/begin", { email });
}

export function verifyAddPasskeyCode(email: string, code: string): Promise<VerifyAddPasskeyCodeResponse> {
  return api.post<VerifyAddPasskeyCodeResponse>("/api/v1/auth/credentials/add/verify", { email, code });
}

export function beginAddPasskeyRegistration(addPasskeyToken: string): Promise<BeginRegistrationResponse> {
  return api.post<BeginRegistrationResponse>("/api/v1/auth/credentials/add/begin-registration", { addPasskeyToken });
}

export function completeAddPasskey(addPasskeyToken: string, attestationJson: string): Promise<void> {
  return api.post<void>("/api/v1/auth/credentials/add/complete", { addPasskeyToken, attestationJson });
}

// --- Credentials ---

export function getCredentials(email: string): Promise<GetCredentialsResponse> {
  return api.get<GetCredentialsResponse>(`/api/v1/auth/credentials?email=${encodeURIComponent(email)}`);
}

export function removeCredential(email: string, credentialId: string): Promise<void> {
  return api.del<void>("/api/v1/auth/credentials", { email, credentialId });
}

// --- Tokens ---

export function refreshToken(token: string): Promise<SessionResult> {
  return api.post<SessionResult>("/api/v1/auth/token/refresh", { refreshToken: token });
}

export function revokeAllTokens(): Promise<void> {
  return api.del<void>("/api/v1/auth/tokens");
}

export { type CredentialInfo, type GetCredentialsResponse };