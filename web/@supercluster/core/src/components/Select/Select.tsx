import type { Component } from "solid-js";
import { createSignal, createEffect, Show, For } from "solid-js";
import { Portal } from "solid-js/web";
import { Icon } from "@/components/Icon";

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface SelectProps {
  options: SelectOption[];
  placeholder: string;
  value?: string;
  disabled?: boolean;
  class?: string;
  onChange?: (value: string) => void;
}

export const Select: Component<SelectProps> = (props) => {
  const [show, setShow] = createSignal(false);
  const [visible, setVisible] = createSignal(false);
  const [currentValue, setCurrentValue] = createSignal(props.value);
  let openCallback: number | undefined;

  createEffect(() => {
    setCurrentValue(props.value);
  });

  function open() {
    if (props.disabled) return;
    setShow(true);
    if (openCallback !== undefined) {
      cancelAnimationFrame(openCallback);
    }
    openCallback = requestAnimationFrame(() => {
      openCallback = requestAnimationFrame(() => setVisible(true));
    });
    document.body.style.overflow = "hidden";
  }

  function close() {
    setVisible(false);
    if (openCallback !== undefined) {
      cancelAnimationFrame(openCallback);
    }
    const timer = setTimeout(() => {
      setShow(false);
      document.body.style.overflow = "";
    }, 200);
    void timer;
  }

  const selected = () => props.options.find((o) => o.value === currentValue());

  return (
    <div class={`relative ${props.disabled ? "opacity-50 cursor-not-allowed" : ""} ${props.class ?? ""}`}>
      <button
        type="button"
        class="w-full px-3 py-2 bg-surface border border-border rounded-field text-left text-high-emphasis placeholder:text-low-emphasis transition-colors duration-150 shadow-none focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent flex items-center justify-between gap-2 cursor-pointer"
        disabled={props.disabled}
        onClick={open}
        aria-haspopup="listbox"
        aria-expanded={show()}
      >
        <span class={selected() ? "text-high-emphasis" : "text-low-emphasis"}>
          {selected()?.label ?? props.placeholder}
        </span>
        <span class={`transition-transform duration-200 shrink-0 flex items-center ${visible() ? "rotate-180" : ""}`}>
          <Icon name="chevron-down" size={16} class="text-medium-emphasis" />
        </span>
      </button>

      <Show when={show()}>
        <Portal mount={document.body}>
          <div class="fixed inset-0 z-50">
            <div
              class={`fixed inset-0 bg-black/60 backdrop-blur-sm transition-opacity duration-200 ${visible() ? "opacity-100" : "opacity-0"}`}
              onClick={close}
            />
            <div
              class={`fixed bottom-0 inset-x-0 z-10 bg-surface-alt border-t border-border rounded-t-card transition-transform duration-200 ${visible() ? "translate-y-0" : "translate-y-full"}`}
              role="listbox"
              onKeyDown={(e) => {
                if (e.key === "Escape") {
                  e.stopPropagation();
                  close();
                }
              }}
            >
              <div class="max-h-[60vh] overflow-y-auto py-2">
                <For each={props.options}>
                  {(option) => (
                    <button
                      type="button"
                      class="w-full text-left px-4 py-3 text-high-emphasis hover:bg-surface flex items-center justify-between gap-3 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                      disabled={option.disabled}
                      role="option"
                      aria-selected={option.value === currentValue()}
                      onClick={() => {
                        close();
                        if (!option.disabled) {
                          props.onChange?.(option.value);
                        }
                      }}
                    >
                      <span>{option.label}</span>
                      {option.value === currentValue() && (
                        <Icon name="check" size={16} class="text-primary shrink-0" />
                      )}
                    </button>
                  )}
                </For>
              </div>
            </div>
          </div>
        </Portal>
      </Show>
    </div>
  );
};