import type { Component, JSX } from "solid-js";
import { Dynamic } from "solid-js/web";

export type TextVariant = "headline" | "subhead" | "body" | "caption";

export interface TextProps {
  variant?: TextVariant;
  as?: "h1" | "h2" | "h3" | "h4" | "h5" | "h6" | "p" | "span";
  class?: string;
  children: JSX.Element;
}

const variantStyles: Record<TextVariant, string> = {
  headline: "text-2xl font-semibold leading-tight",
  subhead: "text-lg font-medium leading-snug",
  body: "text-base leading-relaxed",
  caption: "text-sm leading-normal",
};

const defaultTag: Record<TextVariant, string> = {
  headline: "h2",
  subhead: "h3",
  body: "p",
  caption: "span",
};

export const Text: Component<TextProps> = (props) => {
  const variant = () => props.variant ?? "body";
  const tag = () => props.as ?? defaultTag[variant()];
  const styles = () => `${variantStyles[variant()]} ${props.class ?? ""}`;

  return (
    <Dynamic component={tag()} class={styles()}>
      {props.children}
    </Dynamic>
  );
};