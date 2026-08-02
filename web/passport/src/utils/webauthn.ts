// --- Base64url conversion ---

function arrayBufferToBase64url(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let binary = "";
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function base64urlToArrayBuffer(base64url: string): ArrayBuffer {
  const base64 = base64url.replace(/-/g, "+").replace(/_/g, "/");
  const padding = "=".repeat((4 - (base64.length % 4)) % 4);
  const binary = atob(base64 + padding);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes.buffer;
}

// --- WebAuthn helpers ---

function parseCredentialCreationOptions(optionsJson: string): CredentialCreationOptions {
  const options = JSON.parse(optionsJson);

  if (options.publicKey) {
    options.publicKey.challenge = base64urlToArrayBuffer(options.publicKey.challenge);
    options.publicKey.user.id = base64urlToArrayBuffer(options.publicKey.user.id);

    if (options.publicKey.excludeCredentials) {
      for (const cred of options.publicKey.excludeCredentials) {
        cred.id = base64urlToArrayBuffer(cred.id);
      }
    }
  }

  return options;
}

function parseCredentialRequestOptions(optionsJson: string): CredentialRequestOptions {
  const options = JSON.parse(optionsJson);

  if (options.publicKey) {
    options.publicKey.challenge = base64urlToArrayBuffer(options.publicKey.challenge);

    if (options.publicKey.allowCredentials) {
      for (const cred of options.publicKey.allowCredentials) {
        cred.id = base64urlToArrayBuffer(cred.id);
      }
    }
  }

  return options;
}

function credentialToJson(credential: PublicKeyCredential): string {
  const response = credential.response;

  const json: Record<string, unknown> = {
    id: credential.id,
    rawId: arrayBufferToBase64url(credential.rawId),
    type: credential.type,
    response: {},
  };

  if (response instanceof AuthenticatorAttestationResponse) {
    (json.response as Record<string, unknown>).attestationObject =
      arrayBufferToBase64url(response.attestationObject);
    (json.response as Record<string, unknown>).clientDataJSON =
      arrayBufferToBase64url(response.clientDataJSON);
    if (response.getTransports) {
      (json.response as Record<string, unknown>).transports = response.getTransports();
    }
  } else if (response instanceof AuthenticatorAssertionResponse) {
    (json.response as Record<string, unknown>).authenticatorData =
      arrayBufferToBase64url(response.authenticatorData);
    (json.response as Record<string, unknown>).clientDataJSON =
      arrayBufferToBase64url(response.clientDataJSON);
    (json.response as Record<string, unknown>).signature =
      arrayBufferToBase64url(response.signature);
    if (response.userHandle) {
      (json.response as Record<string, unknown>).userHandle =
        arrayBufferToBase64url(response.userHandle);
    }
  }

  return JSON.stringify(json);
}

// --- Public API ---

export function isPasskeySupported(): boolean {
  return typeof window !== "undefined" && typeof window.PublicKeyCredential !== "undefined";
}

export async function createCredential(optionsJson: string): Promise<string> {
  try {
    const options = parseCredentialCreationOptions(optionsJson);
    const credential = await navigator.credentials.create(options);
    if (!credential) throw new Error("Passkey creation was cancelled.");
    return credentialToJson(credential as PublicKeyCredential);
  } catch (err) {
    console.error("[WebAuthn] createCredential failed:", err);
    throw new Error("Could not create your passkey. Please try again.");
  }
}

export async function getAssertion(optionsJson: string): Promise<string> {
  try {
    const options = parseCredentialRequestOptions(optionsJson);
    const credential = await navigator.credentials.get(options);
    if (!credential) throw new Error("Passkey authentication was cancelled.");
    return credentialToJson(credential as PublicKeyCredential);
  } catch (err) {
    console.error("[WebAuthn] getAssertion failed:", err);
    throw new Error("Could not authenticate your passkey. Please try again.");
  }
}