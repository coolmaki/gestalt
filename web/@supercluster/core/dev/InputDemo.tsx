import { createSignal } from "solid-js";
import { Input } from "@/components/Input";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { FormField } from "@/components/FormField";

export function InputDemo() {
  const [value, setValue] = createSignal("Hello world");
  const [error, setError] = createSignal("");
  const [disabled, setDisabled] = createSignal(false);

  return (
    <div>
      <Text variant="headline" class="mb-6">Input</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex items-start p-12">
            <div class="w-full max-w-sm">
              <FormField label="Demo Input" htmlFor="demo-input" error={error()}>
                <Input
                  id="demo-input"
                  value={value()}
                  error={error()}
                  disabled={disabled()}
                  placeholder="Type something..."
                  onChange={setValue}
                />
              </FormField>
            </div>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <FormField label="Value" htmlFor="inp-value">
              <input
                id="inp-value"
                class="w-full"
                value={value()}
                onInput={(e) => setValue(e.currentTarget.value)}
              />
            </FormField>
            <FormField label="Error message" htmlFor="inp-error">
              <input
                id="inp-error"
                class="w-full"
                placeholder="Leave empty for no error"
                value={error()}
                onInput={(e) => setError(e.currentTarget.value)}
              />
            </FormField>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={disabled()} onChange={(e) => setDisabled(e.currentTarget.checked)} />
              <span class="text-sm">Disabled</span>
            </label>
          </div>
        </Card>
      </div>
    </div>
  );
}