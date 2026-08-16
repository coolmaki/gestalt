import { createSignal, Switch, Match } from "solid-js";
import { useTheme, themeMetaList } from "@/theme-provider";
import type { ThemeKey } from "@/design/themes";
import type { Radius } from "@/theme-provider";
import { Select } from "@/components/Select";
import type { SelectOption } from "@/components/Select";
import { IconDemo } from "./IconDemo";
import { ButtonDemo } from "./ButtonDemo";
import { InputDemo } from "./InputDemo";
import { TextDemo } from "./TextDemo";
import { CardDemo } from "./CardDemo";
import { SelectDemo } from "./SelectDemo";
import { FormFieldDemo } from "./FormFieldDemo";
import { ModalDemo } from "./ModalDemo";

type SectionKey = "icon" | "button" | "input" | "text" | "card" | "select" | "formfield" | "modal";

interface SectionGroup {
  label: string;
  items: { key: SectionKey; label: string }[];
}

const sectionGroups: SectionGroup[] = [
  {
    label: "Display",
    items: [
      { key: "icon", label: "Icon" },
      { key: "text", label: "Text" },
      { key: "card", label: "Card" },
    ],
  },
  {
    label: "Forms",
    items: [
      { key: "button", label: "Button" },
      { key: "input", label: "Input" },
      { key: "select", label: "Select" },
      { key: "formfield", label: "FormField" },
    ],
  },
  {
    label: "Overlay",
    items: [
      { key: "modal", label: "Modal" },
    ],
  },
];

const themeOpts: SelectOption[] = themeMetaList.map((t) => ({ value: t.key, label: t.name }));

const radiusOpts: SelectOption[] = [
  { value: "none", label: "None" },
  { value: "sm", label: "Small" },
  { value: "md", label: "Medium" },
  { value: "lg", label: "Large" },
];

export function App() {
  const [active, setActive] = createSignal<SectionKey>("button");
  const { radius, setRadius, themeKey, setTheme } = useTheme();

  return (
    <div class="flex h-screen">
      <aside class="w-56 shrink-0 border-r border-border bg-surface-alt flex flex-col">
        <div class="px-4 py-4 border-b border-border">
          <h1 class="text-sm font-semibold text-medium-emphasis uppercase tracking-wider">Components</h1>
        </div>
        <nav class="flex-1 overflow-y-auto px-2 py-2">
          {sectionGroups.map((group) => (
            <div class="mb-3">
              <div class="px-3 py-1 text-[0.65rem] font-semibold uppercase tracking-widest text-low-emphasis">
                {group.label}
              </div>
              {group.items.map((item) => (
                <button
                  type="button"
                  class={`w-full text-left px-3 py-1.5 rounded-field text-sm transition-colors mb-0.5 ${active() === item.key
                    ? "bg-primary text-primary-content"
                    : "text-medium-emphasis hover:text-high-emphasis hover:bg-surface"
                    }`}
                  onClick={() => setActive(item.key)}
                >
                  {item.label}
                </button>
              ))}
            </div>
          ))}
        </nav>
        <div class="px-3 py-3 border-t border-border flex flex-col gap-3">
          <div>
            <label class="text-xs text-medium-emphasis mb-1.5 block">Theme</label>
            <Select
              options={themeOpts}
              placeholder="Theme"
              value={themeKey()}
              onChange={(v) => setTheme(v as ThemeKey)}
            />
          </div>
          <div>
            <label class="text-xs text-medium-emphasis mb-1.5 block">Radius</label>
            <Select
              options={radiusOpts}
              placeholder="Radius"
              value={radius()}
              onChange={(v) => setRadius(v as Radius)}
            />
          </div>
        </div>
      </aside>
      <main class="flex-1 overflow-y-auto">
        <div class="max-w-4xl mx-auto px-8 py-8">
          <Switch>
            <Match when={active() === "icon"}><IconDemo /></Match>
            <Match when={active() === "button"}><ButtonDemo /></Match>
            <Match when={active() === "input"}><InputDemo /></Match>
            <Match when={active() === "text"}><TextDemo /></Match>
            <Match when={active() === "card"}><CardDemo /></Match>
            <Match when={active() === "select"}><SelectDemo /></Match>
            <Match when={active() === "formfield"}><FormFieldDemo /></Match>
            <Match when={active() === "modal"}><ModalDemo /></Match>
          </Switch>
        </div>
      </main>
    </div>
  );
}