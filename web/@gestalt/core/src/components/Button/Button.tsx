import type { Component, JSX } from "solid-js";
import { Show } from "solid-js";
import { LoadingIndicator } from "@/components/LoadingIndicator";
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

const filledStyles: Record<ButtonVariant, string> = {
  primary: "bg-primary text-primary-content border-transparent hover:bg-primary-hover hover:text-primary-hover-content active:bg-primary-active active:text-primary-active-content disabled:bg-primary-disabled disabled:text-primary-disabled-content disabled:cursor-not-allowed",
  secondary: "bg-secondary text-secondary-content border-transparent hover:bg-secondary-hover hover:text-secondary-hover-content active:bg-secondary-active active:text-secondary-active-content disabled:bg-secondary-disabled disabled:text-secondary-disabled-content disabled:cursor-not-allowed",
  info: "bg-info text-info-content border-transparent hover:bg-info-hover hover:text-info-hover-content active:bg-info-active active:text-info-active-content disabled:bg-info-disabled disabled:text-info-disabled-content disabled:cursor-not-allowed",
  success: "bg-success text-success-content border-transparent hover:bg-success-hover hover:text-success-hover-content active:bg-success-active active:text-success-active-content disabled:bg-success-disabled disabled:text-success-disabled-content disabled:cursor-not-allowed",
  warning: "bg-warning text-warning-content border-transparent hover:bg-warning-hover hover:text-warning-hover-content active:bg-warning-active active:text-warning-active-content disabled:bg-warning-disabled disabled:text-warning-disabled-content disabled:cursor-not-allowed",
  danger: "bg-danger text-danger-content border-transparent hover:bg-danger-hover hover:text-danger-hover-content active:bg-danger-active active:text-danger-active-content disabled:bg-danger-disabled disabled:text-danger-disabled-content disabled:cursor-not-allowed",
};

const ghostStyles: Record<ButtonVariant, string> = {
  primary: "bg-transparent text-primary border-transparent hover:bg-primary hover:text-primary-content active:bg-primary-active active:text-primary-active-content disabled:opacity-50 disabled:cursor-not-allowed",
  secondary: "bg-transparent text-secondary border-transparent hover:bg-secondary hover:text-secondary-content active:bg-secondary-active active:text-secondary-active-content disabled:opacity-50 disabled:cursor-not-allowed",
  info: "bg-transparent text-info border-transparent hover:bg-info hover:text-info-content active:bg-info-active active:text-info-active-content disabled:opacity-50 disabled:cursor-not-allowed",
  success: "bg-transparent text-success border-transparent hover:bg-success hover:text-success-content active:bg-success-active active:text-success-active-content disabled:opacity-50 disabled:cursor-not-allowed",
  warning: "bg-transparent text-warning border-transparent hover:bg-warning hover:text-warning-content active:bg-warning-active active:text-warning-active-content disabled:opacity-50 disabled:cursor-not-allowed",
  danger: "bg-transparent text-danger border-transparent hover:bg-danger hover:text-danger-content active:bg-danger-active active:text-danger-active-content disabled:opacity-50 disabled:cursor-not-allowed",
};

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

  const styles = () => {
    const variantStyles = fill() === "ghost" ? ghostStyles : filledStyles;
    return `inline-flex items-center justify-center font-medium border rounded-field transition-colors duration-150 cursor-pointer focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-focus focus-visible:ring-offset-2 ${variantStyles[variant()]} ${sizeStyles[size()]} ${props.class ?? ""}`;
  };

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
        <LoadingIndicator size={size() === "sm" ? 16 : 24} />
      </Show>
      {props.children}
    </button>
  );
};
