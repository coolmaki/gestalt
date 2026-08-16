import { createSignal } from "solid-js";
import { Toggle } from "@/components/Toggle";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";

export function ToggleDemo() {
  const [enabled, setEnabled] = createSignal(true);
  const [disabled, setDisabled] = createSignal(false);

  return (
    <div>
      <Text variant="headline" class="mb-6">Toggle</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex flex-col gap-4 p-12">
            <Toggle
              checked={enabled()}
              onChange={setEnabled}
              label="Enable notifications"
            />
            <Toggle
              checked={disabled()}
              onChange={setDisabled}
              label="Permanently disabled"
              disabled
            />
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <div class="text-sm">
              <span class="text-medium-emphasis">Notifications: </span>
              <span class="text-high-emphasis">{enabled() ? "on" : "off"}</span>
            </div>
            <Toggle
              checked={disabled()}
              onChange={setDisabled}
              label="Simulate disabled state"
            />
          </div>
        </Card>
      </div>
    </div>
  );
}
