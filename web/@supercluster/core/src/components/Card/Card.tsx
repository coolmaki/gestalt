import type { Component, JSX } from "solid-js";

export type CardVariant = "default" | "ghost";

export interface CardProps {
  variant?: CardVariant;
  class?: string;
  children: JSX.Element;
}

const variantStyles: Record<CardVariant, string> = {
  default: "bg-surface-alt border border-border rounded-card",
  ghost: "",
};

export const Card: Component<CardProps> = (props) => {
  const styles = () => `${variantStyles[props.variant ?? "default"]} ${props.class ?? ""}`;

  return (
    <div class={styles()}>
      {props.children}
    </div>
  );
};