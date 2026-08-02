import { Component, createSignal } from "solid-js";
import { Button, Text } from "@supercluster/core";
import { useNavigate, useSearchParams } from "@solidjs/router";
import { verifyEmail } from "../../api/auth";
import CodeInput from "../../components/CodeInput";

const VerifyEmail: Component = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.email ?? "";
  const [loading, setLoading] = createSignal(false);
  const [error, setError] = createSignal<string | null>(null);
  const [verified, setVerified] = createSignal(false);

  const handleCodeComplete = async (code: string) => {
    setError(null);
    setLoading(true);
    try {
      await verifyEmail(email, code);
      setVerified(true);
      setTimeout(() => navigate("/auth/login", { replace: true }), 1500);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Verification failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div class="flex flex-col gap-6">
      <Text variant="subhead" class="text-center">Verify your email</Text>

      <Text variant="body" class="text-center text-medium-emphasis">
        {verified()
          ? "Email verified! Redirecting to sign in..."
          : `Enter the 6-digit code sent to ${email || "your email"}`}
      </Text>

      <CodeInput onComplete={handleCodeComplete} disabled={loading() || verified()} />

      {error() && (
        <Text variant="caption" class="text-danger text-center">{error()}</Text>
      )}

      <div class="text-center">
        <a href="/auth/login" class="text-sm text-primary hover:underline">Back to sign in</a>
      </div>
    </div>
  );
};

export default VerifyEmail;