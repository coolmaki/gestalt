import { createSignal } from "solid-js";
import { Text } from "@/components/Text";
import { Card } from "@/components/Card";
import { FormField } from "@/components/FormField";
import { Select } from "@/components/Select";
import type { TextVariant } from "@/components/Text";
import type { SelectOption } from "@/components/Select";

const variantOpts: SelectOption[] = [
  { value: "headline", label: "Headline" },
  { value: "subhead", label: "Subhead" },
  { value: "body", label: "Body" },
  { value: "caption", label: "Caption" },
];

const tagOpts: SelectOption[] = [
  { value: "", label: "Auto (from variant)" },
  { value: "h1", label: "h1" },
  { value: "h2", label: "h2" },
  { value: "h3", label: "h3" },
  { value: "h4", label: "h4" },
  { value: "h5", label: "h5" },
  { value: "h6", label: "h6" },
  { value: "p", label: "p" },
  { value: "span", label: "span" },
];

const sample: Record<TextVariant, string> = {
  headline: "The quick brown fox jumps over the lazy dog",
  subhead: "Section subtitle with medium weight",
  body: "Body text flows naturally across the page, creating a comfortable reading experience for users. Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
  caption: "Auxiliary information, metadata, and small print at a smaller scale.",
};

export function TextDemo() {
  const [variant, setVariant] = createSignal<TextVariant>("body");
  const [asTag, setAsTag] = createSignal("");

  return (
    <div>
      <Text variant="headline" class="mb-6">Text</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="p-12">
            <Text variant={variant()} as={(asTag() as any) || undefined}>
              {sample[variant()]}
            </Text>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <FormField label="Variant" htmlFor="text-variant">
              <Select
                options={variantOpts}
                placeholder="Variant"
                value={variant()}
                onChange={(v) => setVariant(v as TextVariant)}
              />
            </FormField>
            <FormField label="HTML tag override" htmlFor="text-tag">
              <Select
                options={tagOpts}
                placeholder="Tag"
                value={asTag()}
                onChange={setAsTag}
              />
            </FormField>
          </div>
        </Card>
      </div>
    </div>
  );
}