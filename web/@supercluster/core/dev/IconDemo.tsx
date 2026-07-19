import { createSignal } from "solid-js";
import { Icon } from "@/components/Icon";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { FormField } from "@/components/FormField";
import { Icons, type IconName } from "@/components/Icon";

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
              <select
                id="icon-name"
                class="w-full"
                onChange={(e) => setName(e.currentTarget.value as IconName)}
              >
                {Icons.map((n) => <option value={n} selected={name() === n}>{n}</option>)}
              </select>
            </FormField>
            <FormField label="Size" htmlFor="icon-size">
              <input
                id="icon-size"
                type="number"
                class="w-full"
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