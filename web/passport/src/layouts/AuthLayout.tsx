import { Card, Text } from "@gestalt/core";
import { Component, JSX } from "solid-js";
import { isPasskeySupported } from "../utils/webauthn";

interface AuthLayoutProps {
  children?: JSX.Element;
}

const AuthLayout: Component<AuthLayoutProps> = (props) => {
  if (!isPasskeySupported()) {
    window.location.href = "/auth/unsupported";
    return null;
  }

  return (
    <div class="min-h-screen flex items-center justify-center px-4">
      <Card class="w-full max-w-md p-8" variant="ghost">
        <div class="text-center mb-8">
          <Text variant="headline" class="text-primary">Passport</Text>
          <Text variant="caption" class="text-medium-emphasis mt-1">
            Identity server for Gestalt
          </Text>
        </div>
        {props.children}
      </Card>
    </div>
  );
};

export default AuthLayout;