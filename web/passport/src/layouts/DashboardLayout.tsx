import { Button, Text } from "@supercluster/core";
import { Component, JSX, onMount } from "solid-js";
import { revokeAllTokens } from "../api/auth";
import {
  currentEmail,
  logout,
  restoreSession
} from "../signals/auth";

interface DashboardLayoutProps {
  children?: JSX.Element;
}

const DashboardLayout: Component<DashboardLayoutProps> = (props) => {
  onMount(() => {
    const restored = restoreSession();
    if (!restored) {
      window.location.href = "/auth/login";
    }
  });

  const handleLogout = async () => {
    try {
      await revokeAllTokens();
    } catch {
      // Even if revoke fails, log the user out locally
    }
    logout();
  };

  return (
    <div class="min-h-screen">
      <header class="border-b border-border bg-surface-alt">
        <div class="max-w-4xl mx-auto px-4 h-14 flex items-center justify-between">
          <Text variant="subhead" class="text-primary">Passport</Text>
          <div class="flex items-center gap-4">
            <Text variant="caption" class="text-medium-emphasis">{currentEmail()}</Text>
            <Button variant="ghost" size="sm" onClick={handleLogout}>Log out</Button>
          </div>
        </div>
      </header>
      <main class="max-w-4xl mx-auto px-4 py-8">
        {props.children}
      </main>
    </div>
  );
};

export default DashboardLayout;