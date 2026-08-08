import { Component, createSignal } from "solid-js";
import { Button, Input, FormField, Text } from "@gestalt/core";
import { useNavigate } from "@solidjs/router";
import {
  beginRecovery,
  verifyRecoveryCode,
  beginRecoveryRegistration,
  completeRecovery,
} from "../../api/auth";
import { createCredential } from "../../utils/webauthn";
import CodeInput from "../../components/CodeInput";

const Recovery: Component = () => {
  const navigate = useNavigate();
  const [step, setStep] = createSignal<1 | 2 | 3 | 4>(1);
  const [email, setEmail] = createSignal("");
  const [recoveryToken, setRecoveryToken] = createSignal("");
  const [loading, setLoading] = createSignal(false);
  const [error, setError] = createSignal<string | null>(null);
  const [message, setMessage] = createSignal<string | null>(null);

  const handleBegin = async () => {
    setError(null);
    setLoading(true);
    try {
      await beginRecovery(email());
      setMessage("If this email is registered, we sent a recovery code.");
      setStep(2);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to start recovery");
    } finally {
      setLoading(false);
    }
  };

  const handleCodeComplete = async (code: string) => {
    setError(null);
    setLoading(true);
    try {
      const result = await verifyRecoveryCode(email(), code);
      setRecoveryToken(result.recoveryToken);
      setStep(3);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Invalid code");
    } finally {
      setLoading(false);
    }
  };

  const handleCreatePasskey = async () => {
    setError(null);
    setLoading(true);
    try {
      const { optionsJson } = await beginRecoveryRegistration(recoveryToken());
      const attestationJson = await createCredential(optionsJson);
      await completeRecovery(recoveryToken(), attestationJson);
      setStep(4);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create passkey");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div class="flex flex-col gap-6">
      <Text variant="subhead" class="text-center">Recover account</Text>

      {step() === 1 && (
        <>
          <FormField label="Email" htmlFor="recovery-email" error={error() ?? undefined}>
            <Input
              id="recovery-email"
              type="email"
              placeholder="you@example.com"
              value={email()}
              onChange={setEmail}
              disabled={loading()}
            />
          </FormField>
          <Button onClick={handleBegin} loading={loading()} disabled={!email().trim()}>
            Send recovery code
          </Button>
        </>
      )}

      {step() === 2 && (
        <>
          <Text variant="body" class="text-center text-medium-emphasis">
            {message() ?? "Enter the 6-digit code sent to your email"}
          </Text>
          <CodeInput onComplete={handleCodeComplete} disabled={loading()} />
        </>
      )}

      {step() === 3 && (
        <>
          <Text variant="body" class="text-center text-medium-emphasis">
            Create a new passkey to recover your account. Your old passkeys will be removed.
          </Text>
          <Button onClick={handleCreatePasskey} loading={loading()}>
            Create new passkey
          </Button>
        </>
      )}

      {step() === 4 && (
        <Text variant="body" class="text-center text-success">
          Account recovered! Redirecting to sign in...
        </Text>
      )}

      {error() && (
        <Text variant="caption" class="text-danger text-center">{error()}</Text>
      )}

      <div class="text-center">
        <a href="/auth/login" class="text-sm text-primary hover:underline">Back to sign in</a>
      </div>
    </div>
  );
};

export default Recovery;