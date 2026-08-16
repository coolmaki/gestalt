import { createSignal } from "solid-js";
import { Button } from "@/components/Button";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { FormField } from "@/components/FormField";
import { Select } from "@/components/Select";
import { Toggle } from "@/components/Toggle";
import type { ButtonVariant, ButtonFill, ButtonSize } from "@/components/Button";
import type { SelectOption } from "@/components/Select";

const variantOpts: SelectOption[] = [
  { value: "primary", label: "Primary" },
  { value: "secondary", label: "Secondary" },
  { value: "info", label: "Info" },
  { value: "success", label: "Success" },
  { value: "warning", label: "Warning" },
  { value: "danger", label: "Danger" },
];

const emphasisOpts: SelectOption[] = [
  { value: "filled", label: "Filled" },
  { value: "ghost", label: "Ghost" },
];

const sizeOpts: SelectOption[] = [
  { value: "sm", label: "Small" },
  { value: "md", label: "Medium" },
  { value: "lg", label: "Large" },
];

export function ButtonDemo() {
  const [variant, setVariant] = createSignal<ButtonVariant>("primary");
  const [fill, setFill] = createSignal<ButtonFill>("filled");
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
              fill={fill()}
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
              <Select
                options={variantOpts}
                placeholder="Variant"
                value={variant()}
                onChange={(v) => setVariant(v as ButtonVariant)}
              />
            </FormField>
            <FormField label="Fill" htmlFor="btn-fill">
              <Select
                options={emphasisOpts}
                placeholder="Fill"
                value={fill()}
                onChange={(v) => setFill(v as ButtonFill)}
              />
            </FormField>
            <FormField label="Size" htmlFor="btn-size">
              <Select
                options={sizeOpts}
                placeholder="Size"
                value={size()}
                onChange={(v) => setSize(v as ButtonSize)}
              />
            </FormField>
            <Toggle checked={disabled()} onChange={setDisabled} label="Disabled" />
            <Toggle checked={loading()} onChange={setLoading} label="Loading" />
          </div>
        </Card>
      </div>
    </div>
  );
}