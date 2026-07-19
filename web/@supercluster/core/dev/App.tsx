import { createSignal, Switch, Match, For } from "solid-js";
import { useTheme } from "@/theme-provider";
import type { ThemeKey } from "@/design/themes";
import type { Radius } from "@/theme-provider";
import { IconDemo } from "./IconDemo";
import { ButtonDemo } from "./ButtonDemo";
import { InputDemo } from "./InputDemo";
import { TextDemo } from "./TextDemo";
import { CardDemo } from "./CardDemo";
import { FormFieldDemo } from "./FormFieldDemo";
import { ModalDemo } from "./ModalDemo";

type Section = "icon" | "button" | "input" | "text" | "card" | "formfield" | "modal";

const sections: { key: Section; label: string }[] = [
  { key: "icon", label: "Icon" },
  { key: "button", label: "Button" },
  { key: "input", label: "Input" },
  { key: "text", label: "Text" },
  { key: "card", label: "Card" },
  { key: "formfield", label: "FormField" },
  { key: "modal", label: "Modal" },
];

const themeOpts: { key: ThemeKey; label: string }[] = [
  { key: "obsidian", label: "Obsidian" },
  { key: "matrix", label: "Matrix" },
  { key: "pearl", label: "Pearl" },
  { key: "vapor", label: "Vapor" },
];

const radiusOpts: { key: Radius; label: string }[] = [
  { key: "none", label: "None" },
  { key: "sm", label: "Small" },
  { key: "md", label: "Medium" },
  { key: "lg", label: "Large" },
];

export function App() {
  const [active, setActive] = createSignal<Section>("button");
  const themeCtx = useTheme();

  return (
    <div class="flex h-screen">
      <aside class="w-56 shrink-0 border-r border-border bg-surface-alt flex flex-col">
        <div class="px-4 py-4 border-b border-border">
          <h1 class="text-sm font-semibold text-medium-emphasis uppercase tracking-wider">Components</h1>
        </div>
        <nav class="flex-1 px-2 py-2">
          {sections.map((item) => (
            <button
              type="button"
              class={`w-full text-left px-3 py-2 rounded-field text-sm transition-colors mb-0.5 ${
                active() === item.key
                  ? "bg-primary text-primary-content"
                  : "text-medium-emphasis hover:text-high-emphasis hover:bg-surface"
              }`}
              onClick={() => setActive(item.key)}
            >
              {item.label}
            </button>
          ))}
        </nav>
        <div class="px-3 py-3 border-t border-border flex flex-col gap-3">
          <div>
            <label class="text-xs text-medium-emphasis mb-1.5 block">Theme</label>
            <select
              class="w-full px-2 py-1.5 bg-surface border border-border rounded-field text-sm text-high-emphasis"
              value={themeCtx.themeKey}
              onChange={(e) => themeCtx.setTheme(e.currentTarget.value as ThemeKey)}
            >
              <For each={themeOpts}>
                {(t) => <option value={t.key}>{t.label}</option>}
              </For>
            </select>
          </div>
          <div>
            <label class="text-xs text-medium-emphasis mb-1.5 block">Radius</label>
            <select
              class="w-full px-2 py-1.5 bg-surface border border-border rounded-field text-sm text-high-emphasis"
              value={themeCtx.radius}
              onChange={(e) => themeCtx.setRadius(e.currentTarget.value as Radius)}
            >
              <For each={radiusOpts}>
                {(r) => <option value={r.key}>{r.label}</option>}
              </For>
            </select>
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
            <Match when={active() === "formfield"}><FormFieldDemo /></Match>
            <Match when={active() === "modal"}><ModalDemo /></Match>
          </Switch>
        </div>
      </main>
    </div>
  );
}