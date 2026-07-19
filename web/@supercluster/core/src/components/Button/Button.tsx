import type { Component, JSX } from "solid-js";
import { Show } from "solid-js";
import { Icon } from "@/components/Icon";
import type { Variant } from "@/design/variants";

export type ButtonVariant = Variant;
export type ButtonSize = "sm" | "md" | "lg";

export interface ButtonProps {
  variant?: ButtonVariant;
  size?: ButtonSize;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  loading?: boolean;
  class?: string;
  onClick?: (e: MouseEvent) => void;
  children: JSX.Element;
}

const variantStyles: Record<ButtonVariant, string> = {
  primary: "bg-primary text-primary-content hover:bg-primary-hover border-transparent",
  secondary: "bg-surface-alt text-high-emphasis hover:bg-surface border border-border",
  ghost: "bg-transparent text-high-emphasis hover:bg-surface-alt border-transparent",
  info: "bg-info text-info-content border-transparent",
  success: "bg-success text-success-content border-transparent",
  warning: "bg-warning text-warning-content border-transparent",
  danger: "bg-danger text-danger-content hover:bg-danger-hover border-transparent",
};

const sizeStyles: Record<ButtonSize, string> = {
  sm: "px-3 py-1.5 text-sm gap-1.5",
  md: "px-4 py-2 text-base gap-2",
  lg: "px-6 py-3 text-lg gap-2.5",
};

export const Button: Component<ButtonProps> = (props) => {
  const variant = () => props.variant ?? "primary";
  const size = () => props.size ?? "md";
  const isDisabled = () => props.disabled || props.loading;

  const styles = () =>
    `inline-flex items-center justify-center font-medium border rounded-field transition-colors duration-150 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 ${isDisabled() ? "bg-disabled text-disabled-content cursor-not-allowed" : `${variantStyles[variant()]} cursor-pointer`} ${sizeStyles[size()]} ${props.class ?? ""}`;

  return (
    <button
      type={props.type ?? "button"}
      disabled={isDisabled()}
      class={styles()}
      onClick={(e) => {
        if (!isDisabled()) {
          props.onClick?.(e);
        }
      }}
    >
      <Show when={props.loading}>
        <Icon name="loader" class="animate-spin" size={size() === "sm" ? 14 : 16} />
      </Show>
      {props.children}
    </button>
  );
};