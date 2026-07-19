import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, fireEvent } from "@solidjs/testing-library";
import { Select } from "@/components/Select";
import type { SelectOption } from "@/components/Select";

const options: SelectOption[] = [
  { value: "a", label: "Option A" },
  { value: "b", label: "Option B" },
  { value: "c", label: "Option C (disabled)", disabled: true },
];

describe("Select", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    document.body.style.overflow = "";
  });

  it("renders with placeholder when no value", () => {
    render(() => <Select options={options} placeholder="Pick one" />);
    expect(screen.getByText("Pick one")).toBeTruthy();
  });

  it("shows selected option label when value is set", () => {
    render(() => <Select options={options} placeholder="Pick one" value="b" />);
    expect(screen.getByText("Option B")).toBeTruthy();
  });

  it("opens sheet and calls onChange on selection", () => {
    let selected = "";
    render(() => <Select options={options} placeholder="Pick one" onChange={(v) => { selected = v; }} />);

    screen.getByText("Pick one").click();

    const optionA = document.querySelector('[role="option"]');
    expect(optionA).toBeTruthy();
    fireEvent.click(optionA!);

    expect(selected).toBe("a");
  });

  it("closes on backdrop click", () => {
    render(() => <Select options={options} placeholder="Pick one" />);
    screen.getByText("Pick one").click();

    const backdrop = document.querySelector(".backdrop-blur-sm");
    expect(backdrop).toBeTruthy();
    fireEvent.click(backdrop!);
    vi.runAllTimers();

    expect(document.body.style.overflow).not.toBe("hidden");
  });

  it("does not open when disabled", () => {
    render(() => <Select options={options} placeholder="Pick one" disabled />);
    screen.getByText("Pick one").click();

    const backdrop = document.querySelector(".backdrop-blur-sm");
    expect(backdrop).toBeNull();
  });
});