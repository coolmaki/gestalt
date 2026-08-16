import type { Component } from "solid-js";
import { Icon } from "@/components/Icon";

export interface LoadingIndicatorProps {
  size?: number;
  class?: string;
}

export const LoadingIndicator: Component<LoadingIndicatorProps> = (props) => {
  return (
    <Icon
      name="loading-ring-02"
      class={`animate-spin ${props.class ?? ""}`}
      size={props.size ?? 24}
    />
  );
};
