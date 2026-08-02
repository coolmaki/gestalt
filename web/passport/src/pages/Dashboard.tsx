import { Component, createSignal, createResource, For, Show } from "solid-js";
import { Button, Text, Card, Modal, Icon } from "@supercluster/core";
import { getCredentials, removeCredential, type CredentialInfo } from "../api/auth";
import { currentEmail, logout } from "../signals/auth";
import { revokeAllTokens } from "../api/auth";

const Dashboard: Component = () => {
  const [showRemoveModal, setShowRemoveModal] = createSignal(false);
  const [credentialToRemove, setCredentialToRemove] = createSignal<CredentialInfo | null>(null);
  const [removing, setRemoving] = createSignal(false);
  const [error, setError] = createSignal<string | null>(null);

  const [credentials, { refetch }] = createResource(
    () => currentEmail(),
    (email) => email ? getCredentials(email).then((r) => r.credentials) : [],
  );

  const handleRemove = async () => {
    const cred = credentialToRemove();
    const email = currentEmail();
    if (!cred || !email) return;

    setRemoving(true);
    setError(null);
    try {
      await removeCredential(email, cred.credentialId);
      setShowRemoveModal(false);
      setCredentialToRemove(null);
      refetch();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove passkey");
    } finally {
      setRemoving(false);
    }
  };

  const handleLogoutEverywhere = async () => {
    try {
      await revokeAllTokens();
    } catch {
      // Log out locally even if server call fails
    }
    logout();
  };

  const formatDate = (dateStr: string) => {
    try {
      return new Date(dateStr).toLocaleDateString();
    } catch {
      return dateStr;
    }
  };

  return (
    <div class="flex flex-col gap-6">
      <div class="flex items-center justify-between">
        <Text variant="headline" class="text-high-emphasis">Passkeys</Text>
        <Button variant="ghost" size="sm" onClick={handleLogoutEverywhere}>
          Log out everywhere
        </Button>
      </div>

      {error() && (
        <Text variant="caption" class="text-danger">{error()}</Text>
      )}

      <Show when={credentials()?.length === 0}>
        <Card class="p-6 text-center">
          <Text variant="body" class="text-medium-emphasis">No passkeys found.</Text>
        </Card>
      </Show>

      <div class="flex flex-col gap-3">
        <For each={credentials()}>
          {(cred) => (
            <Card class="p-4 flex items-center justify-between">
              <div class="flex items-center gap-3">
                <Icon name="user" size={20} class="text-medium-emphasis" />
                <div>
                  <Text variant="body" class="text-high-emphasis">
                    {cred.deviceName ?? "Unnamed device"}
                  </Text>
                  <Text variant="caption" class="text-low-emphasis">
                    Created {formatDate(cred.createdAt)}
                  </Text>
                </div>
              </div>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setCredentialToRemove(cred);
                  setShowRemoveModal(true);
                }}
              >
                <Icon name="trash" size={16} />
              </Button>
            </Card>
          )}
        </For>
      </div>

      <Modal
        open={showRemoveModal()}
        onClose={() => { setShowRemoveModal(false); setCredentialToRemove(null); }}
        title="Remove passkey"
      >
        <div class="flex flex-col gap-4">
          <Text variant="body" class="text-medium-emphasis">
            Are you sure you want to remove the passkey
            "{credentialToRemove()?.deviceName ?? "Unnamed device"}"?
          </Text>
          <div class="flex gap-3 justify-end">
            <Button
              variant="secondary"
              onClick={() => { setShowRemoveModal(false); setCredentialToRemove(null); }}
              disabled={removing()}
            >
              Cancel
            </Button>
            <Button variant="danger" onClick={handleRemove} loading={removing()}>
              Remove
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default Dashboard;