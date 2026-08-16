import type { Component, JSX } from "solid-js";
import { Show } from "solid-js";

export interface ToggleProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label?: string;
  id?: string;
  disabled?: boolean;
  class?: string;
  name?: string;
}

export const Toggle: Component<ToggleProps> = (props) => {
  return (
    <label
      class={`inline-flex items-center gap-2 ${props.disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"} ${props.class ?? ""}`}
    >
      <input
        id={props.id}
        name={props.name}
        type="checkbox"
        role="switch"
        class="peer sr-only"
        checked={props.checked}
        disabled={props.disabled}
        aria-checked={props.checked}
        onChange={(e) => {
          if (!props.disabled) {
            props.onChange(e.currentTarget.checked);
          }
        }}
      />
      <span
        aria-hidden="true"
        class={`relative inline-flex shrink-0 h-6 w-10 rounded-full transition-colors duration-200 peer-focus-visible:ring-2 peer-focus-visible:ring-primary-focus peer-focus-visible:ring-offset-2 ${props.checked ? "bg-primary" : "bg-border"}`}
      >
        <span
          class={`absolute top-0.5 left-0.5 h-5 w-5 rounded-full bg-surface transition-transform duration-200 ${props.checked ? "translate-x-4" : "translate-x-0"}`}
        />
      </span>
      <Show when={props.label}>
        <span class="text-sm text-high-emphasis">{props.label}</span>
      </Show>
    </label>
  );
};
