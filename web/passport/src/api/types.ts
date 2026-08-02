// --- Request DTOs ---

export interface BeginRegistrationResponse {
  optionsJson: string;
}

export interface CompleteRegistrationRequest {
  email: string;
  attestationJson: string;
}

export interface VerifyEmailRequest {
  email: string;
  code: string;
}

export interface BeginLoginResponse {
  optionsJson: string;
}

export interface CompleteLoginRequest {
  email: string;
  assertionJson: string;
}

export interface SessionResult {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface VerifyRecoveryCodeResponse {
  recoveryToken: string;
}

export interface VerifyAddPasskeyCodeResponse {
  addPasskeyToken: string;
}

export interface BeginRegistrationResponse {
  optionsJson: string;
}

export interface CredentialInfo {
  credentialId: string;
  deviceName: string | null;
  createdAt: string;
}

export interface GetCredentialsResponse {
  credentials: CredentialInfo[];
}

export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  errors?: Record<string, string[]>;
}