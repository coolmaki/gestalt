import type { Component, JSX } from "solid-js";
import { Show, onCleanup, onMount, createEffect, createSignal } from "solid-js";
import { Portal } from "solid-js/web";
import createFocusTrap from "solid-focus-trap";
import { Icon } from "@/components/Icon";

export interface ModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  class?: string;
  children: JSX.Element;
}

export const Modal: Component<ModalProps> = (props) => {
  let containerRef!: HTMLDivElement;

  const [show, setShow] = createSignal(false);
  const [visible, setVisible] = createSignal(false);

  createEffect(() => {
    if (props.open) {
      setShow(true);
      requestAnimationFrame(() => {
        requestAnimationFrame(() => setVisible(true));
      });
    } else {
      setVisible(false);
      const timer = setTimeout(() => setShow(false), 200);
      onCleanup(() => clearTimeout(timer));
    }
  });

  createFocusTrap({
    element: () => containerRef,
    enabled: () => show() && visible(),
    restoreFocus: true,
  });

  onMount(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && show()) {
        e.stopPropagation();
        props.onClose();
      }
    };
    document.addEventListener("keydown", handleKeyDown);
    onCleanup(() => document.removeEventListener("keydown", handleKeyDown));
  });

  createEffect(() => {
    if (show()) {
      document.body.style.overflow = "hidden";
    }
    onCleanup(() => {
      document.body.style.overflow = "";
    });
  });

  const close = () => {
    if (show()) {
      props.onClose();
    }
  };

  return (
    <Show when={show()}>
      <Portal mount={document.body}>
        <div
          class="fixed inset-0 z-50 flex items-center justify-center"
          role="dialog"
          aria-modal="true"
          aria-label={props.title}
        >
          <div
            class={`fixed inset-0 bg-overlay backdrop-blur-sm transition-opacity duration-200 ${visible() ? "opacity-100" : "opacity-0"}`}
            onClick={close}
          />
          <div
            ref={containerRef}
            class={`relative z-10 w-full max-w-md mx-4 bg-surface border border-border rounded-card shadow-2xl p-6 focus:outline-none transition-all duration-200 ${visible() ? "opacity-100 translate-y-0 scale-100" : "opacity-0 translate-y-4 scale-95"} ${props.class ?? ""}`}
            tabindex={-1}
          >
            <div class="flex items-center justify-between mb-4">
              <Show when={props.title}>
                <h2 class="text-lg font-semibold text-high-emphasis">{props.title}</h2>
              </Show>
              <button
                type="button"
                class="ml-auto p-1 rounded-field text-medium-emphasis hover:text-high-emphasis hover:bg-surface-alt transition-colors"
                onClick={close}
                aria-label="Close"
              >
                <Icon name="x" size={20} />
              </button>
            </div>
            {props.children}
          </div>
        </div>
      </Portal>
    </Show>
  );
};