import type { Component, JSX } from "solid-js";
import { Show } from "solid-js";
import { Icon } from "@/components/Icon";
import type { ButtonVariant, ButtonFill } from "@/design/variants";

export type { ButtonVariant, ButtonFill };
export type ButtonSize = "sm" | "md" | "lg";

export interface ButtonProps {
  variant?: ButtonVariant;
  fill?: ButtonFill;
  size?: ButtonSize;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  loading?: boolean;
  class?: string;
  onClick?: (e: MouseEvent) => void;
  children: JSX.Element;
}

function makeVariantStyles(variant: ButtonVariant, fill: ButtonFill): string {
  const v = variant;

  if (fill === "ghost") {
    return [
      `bg-transparent text-${v} border-transparent`,
      `hover:bg-${v} hover:text-${v}-content`,
      `active:bg-${v}-active active:text-${v}-active-content`,
      `disabled:opacity-50 disabled:cursor-not-allowed`,
    ].join(" ");
  }

  return [
    `bg-${v} text-${v}-content border-transparent`,
    `hover:bg-${v}-hover hover:text-${v}-hover-content`,
    `active:bg-${v}-active active:text-${v}-active-content`,
    `disabled:bg-${v}-disabled disabled:text-${v}-disabled-content disabled:cursor-not-allowed`,
  ].join(" ");
}

const sizeStyles: Record<ButtonSize, string> = {
  sm: "px-3 py-1.5 text-sm gap-1.5",
  md: "px-4 py-2 text-base gap-2",
  lg: "px-6 py-3 text-lg gap-2.5",
};

export const Button: Component<ButtonProps> = (props) => {
  const variant = () => props.variant ?? "primary";
  const fill = () => props.fill ?? "filled";
  const size = () => props.size ?? "md";
  const isDisabled = () => props.disabled || props.loading;

  const styles = () =>
    `inline-flex items-center justify-center font-medium border rounded-field transition-colors duration-150 cursor-pointer focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-focus focus-visible:ring-offset-2 ${makeVariantStyles(variant(), fill())} ${sizeStyles[size()]} ${props.class ?? ""}`;

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