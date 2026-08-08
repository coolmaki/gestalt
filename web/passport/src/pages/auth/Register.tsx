import { Component, createSignal } from "solid-js";
import { Button, Input, FormField, Text } from "@gestalt/core";
import { useNavigate } from "@solidjs/router";
import { beginRegistration, completeRegistration } from "../../api/auth";
import { createCredential } from "../../utils/webauthn";

const Register: Component = () => {
  const navigate = useNavigate();
  const [email, setEmail] = createSignal("");
  const [loading, setLoading] = createSignal(false);
  const [error, setError] = createSignal<string | null>(null);

  const handleRegister = async () => {
    setError(null);
    setLoading(true);
    try {
      const { optionsJson } = await beginRegistration(email());
      const attestationJson = await createCredential(optionsJson);
      await completeRegistration(email(), attestationJson);
      navigate(`/auth/verify?email=${encodeURIComponent(email())}`, { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div class="flex flex-col gap-6">
      <Text variant="subhead" class="text-center">Create account</Text>

      <FormField label="Email" htmlFor="register-email" error={error() ?? undefined}>
        <Input
          id="register-email"
          type="email"
          placeholder="you@example.com"
          value={email()}
          onChange={setEmail}
          disabled={loading()}
        />
      </FormField>

      <Button onClick={handleRegister} loading={loading()} disabled={!email().trim()}>
        Create account
      </Button>

      <div class="text-center text-sm">
        <span class="text-medium-emphasis">Already have an account?</span>
        {" "}
        <a href="/auth/login" class="text-primary hover:underline">Sign in</a>
      </div>
    </div>
  );
};

export default Register;