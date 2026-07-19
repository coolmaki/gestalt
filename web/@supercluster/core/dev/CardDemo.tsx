import { createSignal } from "solid-js";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { Button } from "@/components/Button";
import { FormField } from "@/components/FormField";
import type { CardVariant } from "@/components/Card";

export function CardDemo() {
  const [variant, setVariant] = createSignal<CardVariant>("default");

  return (
    <div>
      <Text variant="headline" class="mb-6">Card</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card variant={variant()}>
          <div class="p-6">
            <Text variant="subhead" class="mb-2">Sample Card</Text>
            <Text variant="body" class="text-medium-emphasis mb-4">
              Cards group related content and provide visual separation from the background.
            </Text>
            <div class="flex gap-3">
              <Button variant="primary" size="sm">Primary</Button>
              <Button variant="secondary" size="sm">Secondary</Button>
            </div>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <FormField label="Variant" htmlFor="card-variant">
              <select
                id="card-variant"
                class="w-full"
                onChange={(e) => setVariant(e.currentTarget.value as CardVariant)}
              >
                <option value="default" selected={variant() === "default"}>Default</option>
                <option value="ghost" selected={variant() === "ghost"}>Ghost</option>
              </select>
            </FormField>
          </div>
        </Card>
      </div>
    </div>
  );
}