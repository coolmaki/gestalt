import type { Component } from "solid-js";
import { createSignal, createEffect, createMemo, Show, For, onCleanup } from "solid-js";
import { Portal } from "solid-js/web";
import { Icon } from "@/components/Icon";
import { Input } from "@/components/Input";
import { LoadingIndicator } from "@/components/LoadingIndicator";

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface SelectProps {
  options?: SelectOption[];
  source?: (query: string) => Promise<SelectOption[]>;
  placeholder: string;
  value?: string;
  disabled?: boolean;
  searchable?: boolean;
  limit?: number;
  debounceMs?: number;
  class?: string;
  onChange?: (value: string) => void;
}

export const Select: Component<SelectProps> = (props) => {
  const [show, setShow] = createSignal(false);
  const [visible, setVisible] = createSignal(false);
  const [currentValue, setCurrentValue] = createSignal(props.value);
  const [query, setQuery] = createSignal("");
  const [debouncedQuery, setDebouncedQuery] = createSignal("");
  const [results, setResults] = createSignal<SelectOption[]>([]);
  const [loading, setLoading] = createSignal(false);
  const [selectedLabel, setSelectedLabel] = createSignal<string | undefined>(undefined);
  const [visibleCount, setVisibleCount] = createSignal(0);
  let openCallback: number | undefined;
  let requestId = 0;

  createEffect(() => {
    setCurrentValue(props.value);
  });

  const hasSource = () => props.source !== undefined;
  const hasSearch = () => props.searchable === true || props.source !== undefined;

  createEffect(() => {
    const q = query();
    const timer = setTimeout(() => setDebouncedQuery(q), props.debounceMs ?? 200);
    onCleanup(() => clearTimeout(timer));
  });

  createEffect(() => {
    if (!props.source) return;
    if (!show()) {
      setLoading(false);
      return;
    }
    const q = debouncedQuery();
    const id = ++requestId;
    setLoading(true);
    props.source(q)
      .then((opts) => {
        if (id === requestId) {
          setResults(opts);
          setLoading(false);
        }
      })
      .catch(() => {
        if (id === requestId) {
          setResults([]);
          setLoading(false);
        }
      });
  });

  const filteredOptions = createMemo<SelectOption[]>(() => {
    if (hasSource()) {
      return results();
    }
    const opts = props.options ?? [];
    const q = query().trim().toLowerCase();
    if (!hasSearch() || !q) {
      return opts;
    }
    return opts.filter((o) => o.label.toLowerCase().includes(q));
  });

  createEffect(() => {
    filteredOptions();
    setVisibleCount(props.limit && props.limit > 0 ? props.limit : Number.MAX_SAFE_INTEGER);
  });

  const visibleOptions = createMemo<SelectOption[]>(() => filteredOptions().slice(0, visibleCount()));
  const remaining = createMemo(() => Math.max(0, filteredOptions().length - visibleOptions().length));

  createEffect(() => {
    const value = currentValue();
    if (!value) {
      setSelectedLabel(undefined);
      return;
    }
    const list = hasSource() ? results() : (props.options ?? []);
    const match = list.find((o) => o.value === value);
    if (match) {
      setSelectedLabel(match.label);
    }
  });

  function open() {
    if (props.disabled) return;
    setQuery("");
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

  return (
    <div class={`relative ${props.disabled ? "opacity-50 cursor-not-allowed" : ""} ${props.class ?? ""}`}>
      <button
        type="button"
        class="w-full px-3 py-2 bg-surface border border-border rounded-field text-left text-high-emphasis placeholder:text-low-emphasis transition-colors duration-150 shadow-none focus:outline-none focus:ring-2 focus:ring-primary-focus focus:border-transparent flex items-center justify-between gap-2 cursor-pointer"
        disabled={props.disabled}
        onClick={open}
        aria-haspopup="listbox"
        aria-expanded={show()}
      >
        <span class={selectedLabel() ? "text-high-emphasis" : "text-low-emphasis"}>
          {selectedLabel() ?? props.placeholder}
        </span>
        <span class={`transition-transform duration-200 shrink-0 flex items-center ${visible() ? "rotate-180" : ""}`}>
          <Icon name="arrow-down" size={16} class="text-medium-emphasis" />
        </span>
      </button>

      <Show when={show()}>
        <Portal mount={document.body}>
          <div class="fixed inset-0 z-50">
            <div
              class={`fixed inset-0 bg-overlay backdrop-blur-sm transition-opacity duration-200 ${visible() ? "opacity-100" : "opacity-0"}`}
              onClick={close}
            />
            <div
              class={`fixed bottom-0 inset-x-0 z-10 bg-surface-alt border-t border-border rounded-t-card overflow-hidden transition-transform duration-200 ${visible() ? "translate-y-0" : "translate-y-full"}`}
              role="listbox"
              onKeyDown={(e) => {
                if (e.key === "Escape") {
                  e.stopPropagation();
                  close();
                }
              }}
            >
              <Show when={hasSearch()}>
                <div class="px-4 pt-3 pb-2">
                  <Input
                    placeholder="Search..."
                    value={query()}
                    onChange={setQuery}
                  />
                </div>
              </Show>

              <div class="max-h-[60vh] overflow-y-auto py-2">
                <Show
                  when={loading()}
                  fallback={
                    <>
                      <For each={visibleOptions()}>
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
                              <Icon name="tick" size={16} class="text-primary shrink-0" />
                            )}
                          </button>
                        )}
                      </For>

                      <Show when={filteredOptions().length === 0}>
                        <div class="px-4 py-3 text-low-emphasis text-sm">No results</div>
                      </Show>

                      <Show when={remaining() > 0}>
                        <button
                          type="button"
                          class="w-full text-left px-4 py-3 text-primary text-sm hover:bg-surface cursor-pointer"
                          onClick={() => setVisibleCount((c) => c + (props.limit ?? remaining()))}
                        >
                          Show {Math.min(props.limit ?? remaining(), remaining())} more
                        </button>
                      </Show>
                    </>
                  }
                >
                  <div class="flex items-center justify-center py-6 text-medium-emphasis">
                    <LoadingIndicator size={32} />
                  </div>
                </Show>
              </div>
            </div>
          </div>
        </Portal>
      </Show>
    </div>
  );
};
