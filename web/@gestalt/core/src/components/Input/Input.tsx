import type { Component } from "solid-js";
import { splitProps } from "solid-js";

export interface InputProps {
  id?: string;
  type?: "text" | "email" | "password" | "number" | "search" | "tel" | "url";
  placeholder?: string;
  value?: string;
  error?: string;
  disabled?: boolean;
  class?: string;
  onChange?: (value: string) => void;
  onInput?: (value: string) => void;
}

export const Input: Component<InputProps> = (props) => {
  const [local] = splitProps(props, ["id", "type", "placeholder", "value", "error", "disabled", "class", "onChange", "onInput"]);

  const styles = () =>
    `w-full px-3 py-2 bg-surface border rounded-field text-high-emphasis placeholder:text-low-emphasis transition-colors duration-150 focus:outline-none focus:ring-2 focus:ring-primary-focus focus:border-transparent disabled:bg-disabled disabled:text-disabled-content disabled:cursor-not-allowed ${local.error ? "border-danger focus:ring-danger" : "border-border"} ${local.class ?? ""}`;

  return (
    <input
      id={local.id}
      type={local.type ?? "text"}
      placeholder={local.placeholder}
      value={local.value ?? ""}
      disabled={local.disabled}
      class={styles()}
      onInput={(e) => {
        local.onInput?.(e.currentTarget.value);
        local.onChange?.(e.currentTarget.value);
      }}
    />
  );
};