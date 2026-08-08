import { Component } from "solid-js";
import { Text, Button } from "@gestalt/core";

const Unsupported: Component = () => {
  return (
    <div class="flex flex-col gap-6 items-center text-center">
      <Text variant="subhead" class="text-danger">Browser not supported</Text>
      <Text variant="body" class="text-medium-emphasis">
        This browser doesn't support passkeys (WebAuthn). Passport requires a modern
        browser with WebAuthn support to authenticate securely.
      </Text>
      <Text variant="body" class="text-medium-emphasis">
        Please try:
      </Text>
      <ul class="text-medium-emphasis text-sm list-disc list-inside">
        <li>Chrome 109+</li>
        <li>Firefox 120+</li>
        <li>Safari 16+</li>
        <li>Edge 109+</li>
      </ul>
      <Button
        variant="secondary"
        onClick={() => window.location.reload()}
      >
        Try again
      </Button>
    </div>
  );
};

export default Unsupported;