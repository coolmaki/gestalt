import { Component, createSignal, createEffect, For, onMount } from "solid-js";
import { Input, Text } from "@gestalt/core";

interface CodeInputProps {
  length?: number;
  onComplete: (code: string) => void;
  disabled?: boolean;
}

const CodeInput: Component<CodeInputProps> = (props) => {
  const length = props.length ?? 6;
  const [digits, setDigits] = createSignal<string[]>(Array(length).fill(""));
  let inputRefs: HTMLInputElement[] = [];

  const handleInput = (index: number, value: string) => {
    // Only allow single digit
    const digit = value.replace(/\D/g, "").slice(-1);
    const newDigits = [...digits()];
    newDigits[index] = digit;
    setDigits(newDigits);

    if (digit && index < length - 1) {
      inputRefs[index + 1]?.focus();
    }

    const code = newDigits.join("");
    if (newDigits.every((d) => d !== "")) {
      props.onComplete(code);
    }
  };

  const handleKeyDown = (index: number, e: KeyboardEvent) => {
    if (e.key === "Backspace" && !digits()[index] && index > 0) {
      inputRefs[index - 1]?.focus();
    }
  };

  const handlePaste = (e: ClipboardEvent) => {
    e.preventDefault();
    const pasted = e.clipboardData?.getData("text").replace(/\D/g, "").slice(0, length);
    if (!pasted) return;

    const newDigits = Array(length).fill("");
    for (let i = 0; i < pasted.length; i++) {
      newDigits[i] = pasted[i];
    }
    setDigits(newDigits);

    const code = newDigits.join("");
    if (newDigits.every((d) => d !== "")) {
      props.onComplete(code);
    }

    const nextEmptyIndex = newDigits.findIndex((d) => !d);
    const focusIndex = nextEmptyIndex === -1 ? length - 1 : nextEmptyIndex;
    inputRefs[focusIndex]?.focus();
  };

  return (
    <div class="flex gap-2 justify-center">
      <For each={Array(length)}>
        {(_, index) => (
          <input
            ref={(el) => { inputRefs[index()] = el; }}
            type="text"
            inputmode="numeric"
            maxLength={1}
            value={digits()[index()]}
            onInput={(e) => handleInput(index(), e.currentTarget.value)}
            onKeyDown={(e) => handleKeyDown(index(), e)}
            onPaste={index() === 0 ? handlePaste : undefined}
            disabled={props.disabled}
            class="w-10 h-12 text-center text-lg font-semibold bg-surface border border-border rounded-field text-high-emphasis focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent disabled:bg-disabled disabled:text-disabled-content"
          />
        )}
      </For>
    </div>
  );
};

export default CodeInput;