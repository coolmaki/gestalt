import { Component, createSignal } from "solid-js";
import { Button, Input, FormField, Text } from "@gestalt/core";
import { useNavigate } from "@solidjs/router";
import { beginLogin, completeLogin } from "../../api/auth";
import { loginFromSession } from "../../signals/auth";
import { getAssertion, isPasskeySupported } from "../../utils/webauthn";

const Login: Component = () => {
  const navigate = useNavigate();
  const [email, setEmail] = createSignal("");
  const [step, setStep] = createSignal<"email" | "passkey">("email");
  const [loading, setLoading] = createSignal(false);
  const [error, setError] = createSignal<string | null>(null);

  const handleEmailSubmit = async () => {
    setError(null);
    setLoading(true);
    try {
      const { optionsJson } = await beginLogin(email());
      const assertionJson = await getAssertion(optionsJson);
      const result = await completeLogin(email(), assertionJson);
      loginFromSession(result.accessToken, result.refreshToken);
      navigate("/dashboard", { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div class="flex flex-col gap-6">
      <Text variant="subhead" class="text-center">Sign in</Text>

      <FormField label="Email" htmlFor="login-email" error={error() ?? undefined}>
        <Input
          id="login-email"
          type="email"
          placeholder="you@example.com"
          value={email()}
          onChange={setEmail}
          disabled={loading()}
        />
      </FormField>

      <Button onClick={handleEmailSubmit} loading={loading()} disabled={!email().trim()}>
        Continue
      </Button>

      <div class="text-center text-sm">
        <a href="/auth/register" class="text-primary hover:underline">Create an account</a>
        <span class="mx-2 text-medium-emphasis">·</span>
        <a href="/auth/recovery" class="text-primary hover:underline">Recover account</a>
      </div>
    </div>
  );
};

export default Login;