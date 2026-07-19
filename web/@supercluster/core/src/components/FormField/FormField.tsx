import type { Component, JSX } from "solid-js";
import { Show } from "solid-js";

export interface FormFieldProps {
  label: string;
  error?: string;
  htmlFor: string;
  class?: string;
  children: JSX.Element;
}

export const FormField: Component<FormFieldProps> = (props) => {
  return (
    <div class={`flex flex-col gap-1 ${props.class ?? ""}`}>
      <label
        for={props.htmlFor}
        class="text-sm font-medium text-medium-emphasis"
      >
        {props.label}
      </label>
      {props.children}
      <Show when={props.error}>
        <div class="bg-danger text-danger-content rounded-field px-3 py-1.5 text-sm">{props.error}</div>
      </Show>
    </div>
  );
};