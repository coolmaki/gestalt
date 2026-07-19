import { createSignal } from "solid-js";
import { Button } from "@/components/Button";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { FormField } from "@/components/FormField";
import type { ButtonVariant, ButtonSize } from "@/components/Button";

const variantOpts: ButtonVariant[] = ["primary", "secondary", "ghost", "info", "success", "warning", "danger"];
const sizeOpts: ButtonSize[] = ["sm", "md", "lg"];

export function ButtonDemo() {
  const [variant, setVariant] = createSignal<ButtonVariant>("primary");
  const [size, setSize] = createSignal<ButtonSize>("md");
  const [disabled, setDisabled] = createSignal(false);
  const [loading, setLoading] = createSignal(false);
  const [clicks, setClicks] = createSignal(0);

  return (
    <div>
      <Text variant="headline" class="mb-6">Button</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex items-center justify-center gap-4 p-12">
            <Button
              variant={variant()}
              size={size()}
              disabled={disabled()}
              loading={loading()}
              onClick={() => setClicks((c) => c + 1)}
            >
              {loading() ? "Loading..." : "Button"}
            </Button>
            <span class="text-sm text-medium-emphasis">Clicked {clicks()} times</span>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <FormField label="Variant" htmlFor="btn-variant">
              <select
                id="btn-variant"
                class="w-full"
                onChange={(e) => setVariant(e.currentTarget.value as ButtonVariant)}
              >
                {variantOpts.map((v) => <option value={v} selected={variant() === v}>{v}</option>)}
              </select>
            </FormField>
            <FormField label="Size" htmlFor="btn-size">
              <select
                id="btn-size"
                class="w-full"
                onChange={(e) => setSize(e.currentTarget.value as ButtonSize)}
              >
                {sizeOpts.map((s) => <option value={s} selected={size() === s}>{s}</option>)}
              </select>
            </FormField>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={disabled()} onChange={(e) => setDisabled(e.currentTarget.checked)} />
              <span class="text-sm">Disabled</span>
            </label>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={loading()} onChange={(e) => setLoading(e.currentTarget.checked)} />
              <span class="text-sm">Loading</span>
            </label>
          </div>
        </Card>
      </div>
    </div>
  );
}