import { createSignal } from "solid-js";
import { Select } from "@/components/Select";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { FormField } from "@/components/FormField";
import type { SelectOption } from "@/components/Select";

const fruitOptions: SelectOption[] = [
  { value: "apple", label: "Apple" },
  { value: "apricot", label: "Apricot" },
  { value: "banana", label: "Banana" },
  { value: "blackberry", label: "Blackberry" },
  { value: "blueberry", label: "Blueberry" },
  { value: "cherry", label: "Cherry" },
  { value: "coconut", label: "Coconut" },
  { value: "cranberry", label: "Cranberry" },
  { value: "date", label: "Date" },
  { value: "elderberry", label: "Elderberry" },
  { value: "fig", label: "Fig" },
  { value: "grape", label: "Grape" },
  { value: "grapefruit", label: "Grapefruit" },
  { value: "honeydew", label: "Honeydew" },
  { value: "kiwi", label: "Kiwi" },
  { value: "lemon", label: "Lemon" },
  { value: "lime", label: "Lime" },
  { value: "mango", label: "Mango" },
  { value: "orange", label: "Orange" },
  { value: "papaya", label: "Papaya" },
  { value: "peach", label: "Peach" },
  { value: "pear", label: "Pear" },
  { value: "pineapple", label: "Pineapple" },
  { value: "plum", label: "Plum" },
  { value: "raspberry", label: "Raspberry" },
  { value: "strawberry", label: "Strawberry" },
  { value: "watermelon", label: "Watermelon" },
];

const countries: SelectOption[] = Array.from({ length: 30 }, (_, i) => ({
  value: `c${i}`,
  label: `Country ${i + 1}`,
}));

const mockSource = (query: string): Promise<SelectOption[]> =>
  new Promise((resolve) => {
    setTimeout(() => {
      resolve(
        Array.from({ length: 8 }, (_, i) => ({
          value: `s-${query}-${i}`,
          label: `Result ${query || "all"}-${i + 1}`,
        })),
      );
    }, 400);
  });

export function SelectDemo() {
  const [fruit, setFruit] = createSignal("");
  const [country, setCountry] = createSignal("");
  const [sourceValue, setSourceValue] = createSignal("");

  return (
    <div>
      <Text variant="headline" class="mb-6">Select</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex flex-col gap-6 p-6">
            <FormField label="Searchable (client-side filter)" htmlFor="select-searchable">
              <Select
                options={fruitOptions}
                placeholder="Search fruits..."
                searchable
                value={fruit()}
                onChange={setFruit}
              />
            </FormField>

            <FormField label="Limited (shows 5 at a time)" htmlFor="select-limit">
              <Select
                options={countries}
                placeholder="Choose a country..."
                limit={5}
                value={country()}
                onChange={setCountry}
              />
            </FormField>

            <FormField label="Async source (mock 400ms fetch)" htmlFor="select-source">
              <Select
                source={mockSource}
                placeholder="Type to search..."
                value={sourceValue()}
                onChange={setSourceValue}
              />
            </FormField>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <div class="text-sm">
              <span class="text-medium-emphasis">Fruit: </span>
              <span class="text-high-emphasis">{fruit() || "none"}</span>
            </div>
            <div class="text-sm">
              <span class="text-medium-emphasis">Country: </span>
              <span class="text-high-emphasis">{country() || "none"}</span>
            </div>
            <div class="text-sm">
              <span class="text-medium-emphasis">Source: </span>
              <span class="text-high-emphasis">{sourceValue() || "none"}</span>
            </div>
            <div class="text-sm text-medium-emphasis">
              <strong>Features:</strong><br />
              Bottom sheet with backdrop<br />
              Checkmark on selected option<br />
              Search box (searchable / source)<br />
              "Show N more" limit reveal<br />
              Debounced async source (200ms)<br />
              Backdrop or Escape to dismiss
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}
