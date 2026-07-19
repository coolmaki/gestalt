import { createSignal } from "solid-js";
import { Select } from "@/components/Select";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { FormField } from "@/components/FormField";
import type { SelectOption } from "@/components/Select";

const options: SelectOption[] = [
  { value: "apple", label: "Apple" },
  { value: "banana", label: "Banana" },
  { value: "cherry", label: "Cherry" },
  { value: "date", label: "Date" },
  { value: "elderberry", label: "Elderberry" },
  { value: "fig", label: "Fig" },
  { value: "grape", label: "Grape (disabled)", disabled: true },
  { value: "honeydew", label: "Honeydew" },
];

export function SelectDemo() {
  const [value, setValue] = createSignal("");

  return (
    <div>
      <Text variant="headline" class="mb-6">Select</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex items-start p-12">
            <div class="w-full max-w-sm">
              <FormField label="Fruit" htmlFor="select-demo">
                <Select
                  options={options}
                  placeholder="Choose a fruit..."
                  value={value()}
                  onChange={setValue}
                />
              </FormField>
            </div>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <div class="text-sm">
              <span class="text-medium-emphasis">Selected: </span>
              <span class="text-high-emphasis">{value() || "none"}</span>
            </div>
            <div class="text-sm text-medium-emphasis">
              <strong>Features:</strong><br />
              Bottom sheet with backdrop<br />
              Checkmark on selected option<br />
              Scrollable list (max 60vh)<br />
              Backdrop or Escape to dismiss<br />
              Disabled options supported
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}