import { createSignal } from "solid-js";
import { Text } from "@/components/Text";
import { Card } from "@/components/Card";
import { FormField } from "@/components/FormField";
import type { TextVariant } from "@/components/Text";

const variantOpts: { key: TextVariant; label: string }[] = [
  { key: "headline", label: "Headline" },
  { key: "subhead", label: "Subhead" },
  { key: "body", label: "Body" },
  { key: "caption", label: "Caption" },
];

const tagOpts = ["", "h1", "h2", "h3", "h4", "h5", "h6", "p", "span"] as const;

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
              <select
                id="text-variant"
                class="w-full"
                onChange={(e) => setVariant(e.currentTarget.value as TextVariant)}
              >
                {variantOpts.map((v) => <option value={v.key} selected={variant() === v.key}>{v.label}</option>)}
              </select>
            </FormField>
            <FormField label="HTML tag override" htmlFor="text-tag">
              <select
                id="text-tag"
                class="w-full"
                onChange={(e) => setAsTag(e.currentTarget.value)}
              >
                <option value="" selected={asTag() === ""}>Auto (from variant)</option>
                {tagOpts.filter(Boolean).map((t) => <option value={t} selected={asTag() === t}>{t}</option>)}
              </select>
            </FormField>
          </div>
        </Card>
      </div>
    </div>
  );
}