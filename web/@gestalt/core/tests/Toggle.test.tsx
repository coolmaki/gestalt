import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@solidjs/testing-library";
import { Toggle } from "@/components/Toggle";

describe("Toggle", () => {
  it("renders label text", () => {
    render(() => <Toggle checked={false} onChange={() => {}} label="Enable notifications" />);
    expect(screen.getByText("Enable notifications")).toBeTruthy();
  });

  it("renders without label", () => {
    render(() => <Toggle checked={false} onChange={() => {}} />);
    const input = document.querySelector('input[type="checkbox"]');
    expect(input).toBeTruthy();
  });

  it("reflects checked state", () => {
    render(() => <Toggle checked={true} onChange={() => {}} />);
    const input = document.querySelector('input[type="checkbox"]') as HTMLInputElement;
    expect(input.checked).toBe(true);
    expect(input.getAttribute("role")).toBe("switch");
    expect(input.getAttribute("aria-checked")).toBe("true");
  });

  it("reflects unchecked state", () => {
    render(() => <Toggle checked={false} onChange={() => {}} />);
    const input = document.querySelector('input[type="checkbox"]') as HTMLInputElement;
    expect(input.checked).toBe(false);
    expect(input.getAttribute("aria-checked")).toBe("false");
  });

  it("calls onChange with true on click when unchecked", () => {
    const fn = vi.fn();
    render(() => <Toggle checked={false} onChange={fn} />);
    const input = document.querySelector('input[type="checkbox"]') as HTMLInputElement;
    fireEvent.click(input);
    expect(fn).toHaveBeenCalledWith(true);
  });

  it("calls onChange with false on click when checked", () => {
    const fn = vi.fn();
    render(() => <Toggle checked={true} onChange={fn} />);
    const input = document.querySelector('input[type="checkbox"]') as HTMLInputElement;
    fireEvent.click(input);
    expect(fn).toHaveBeenCalledWith(false);
  });

  it("does not call onChange when disabled", () => {
    const fn = vi.fn();
    render(() => <Toggle checked={false} onChange={fn} disabled />);
    const input = document.querySelector('input[type="checkbox"]') as HTMLInputElement;
    fireEvent.click(input);
    expect(fn).not.toHaveBeenCalled();
  });

  it("applies custom class", () => {
    render(() => <Toggle checked={false} onChange={() => {}} class="my-toggle" />);
    const label = document.querySelector("label");
    expect(label?.classList.contains("my-toggle")).toBe(true);
  });
});