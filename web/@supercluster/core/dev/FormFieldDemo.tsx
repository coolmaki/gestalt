import { createSignal } from "solid-js";
import { FormField } from "@/components/FormField";
import { Input } from "@/components/Input";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";

export function FormFieldDemo() {
  const [error, setError] = createSignal("");
  const [disabled, setDisabled] = createSignal(false);

  return (
    <div>
      <Text variant="headline" class="mb-6">FormField</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex items-start p-12">
            <div class="w-full max-w-sm">
              <FormField label="Email address" htmlFor="ff-email" error={error()}>
                <Input
                  id="ff-email"
                  placeholder="you@example.com"
                  disabled={disabled()}
                  error={error()}
                />
              </FormField>
            </div>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <FormField label="Error message" htmlFor="ff-error">
              <input
                id="ff-error"
                class="w-full"
                placeholder="Set error text"
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