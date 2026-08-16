import { createSignal } from "solid-js";
import { Icon } from "@/components/Icon";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { FormField } from "@/components/FormField";
import { Select } from "@/components/Select";
import { Icons, type IconName } from "@/components/Icon";
import type { SelectOption } from "@/components/Select";

const nameOpts: SelectOption[] = Icons.map((n) => ({ value: n, label: n }));

export function IconDemo() {
  const [name, setName] = createSignal<IconName>("check");
  const [size, setSize] = createSignal(24);

  return (
    <div>
      <Text variant="headline" class="mb-6">Icon</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex items-center justify-center p-12">
            <Icon name={name()} size={size()} />
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <FormField label="Name" htmlFor="icon-name">
              <Select
                options={nameOpts}
                placeholder="Choose an icon"
                value={name()}
                onChange={(v) => setName(v as IconName)}
              />
            </FormField>
            <FormField label="Size" htmlFor="icon-size">
              <input
                id="icon-size"
                type="number"
                class="w-full px-3 py-2 bg-surface border border-border rounded-field text-high-emphasis placeholder:text-low-emphasis transition-colors duration-150 focus:outline-none focus:ring-2 focus:ring-primary-focus focus:border-transparent"
                min={12}
                max={96}
                value={size()}
                onInput={(e) => setSize(Number(e.currentTarget.value))}
              />
            </FormField>
          </div>
        </Card>
      </div>
    </div>
  );
}