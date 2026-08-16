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

describe("Select with searchable", () => {
  afterEach(() => {
    document.body.style.overflow = "";
  });

  const fruitOptions: SelectOption[] = [
    { value: "a", label: "Apple" },
    { value: "b", label: "Banana" },
    { value: "c", label: "Cherry" },
  ];

  it("filters options by query", () => {
    render(() => <Select options={fruitOptions} placeholder="Pick" searchable />);
    screen.getByText("Pick").click();

    const input = document.querySelector('input[placeholder="Search..."]') as HTMLInputElement;
    input.value = "ban";
    input.dispatchEvent(new Event("input", { bubbles: true }));

    const optionLabels = [...document.querySelectorAll('[role="option"]')].map((el) => el.textContent?.trim());
    expect(optionLabels).toEqual(["Banana"]);
  });

  it("shows 'No results' when query matches nothing", () => {
    render(() => <Select options={fruitOptions} placeholder="Pick" searchable />);
    screen.getByText("Pick").click();

    const input = document.querySelector('input[placeholder="Search..."]') as HTMLInputElement;
    input.value = "zzz";
    input.dispatchEvent(new Event("input", { bubbles: true }));

    expect(screen.getByText("No results")).toBeTruthy();
  });
});

describe("Select with limit", () => {
  afterEach(() => {
    document.body.style.overflow = "";
  });

  const manyOptions: SelectOption[] = [
    { value: "1", label: "One" },
    { value: "2", label: "Two" },
    { value: "3", label: "Three" },
    { value: "4", label: "Four" },
    { value: "5", label: "Five" },
  ];

  it("caps options and shows 'Show N more'", () => {
    render(() => <Select options={manyOptions} placeholder="Pick" limit={3} />);
    screen.getByText("Pick").click();

    const optionLabels = [...document.querySelectorAll('[role="option"]')].map((el) => el.textContent?.trim());
    expect(optionLabels).toEqual(["One", "Two", "Three"]);

    const showMore = screen.getByText("Show 2 more");
    expect(showMore).toBeTruthy();
  });

  it("reveals more options on 'Show N more' click", () => {
    render(() => <Select options={manyOptions} placeholder="Pick" limit={3} />);
    screen.getByText("Pick").click();

    screen.getByText("Show 2 more").click();

    const optionLabels = [...document.querySelectorAll('[role="option"]')].map((el) => el.textContent?.trim());
    expect(optionLabels).toEqual(["One", "Two", "Three", "Four", "Five"]);
  });
});

describe("Select with source", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    document.body.style.overflow = "";
  });

  it("fetches from source on open and renders results", async () => {
    const source = vi.fn((_q: string) =>
      Promise.resolve([
        { value: "1", label: "Result A" },
        { value: "2", label: "Result B" },
      ]),
    );

    render(() => <Select source={source} placeholder="Search" />);
    screen.getByText("Search").click();

    expect(source).toHaveBeenCalledWith("");

    await Promise.resolve();

    expect(screen.getByText("Result A")).toBeTruthy();
    expect(screen.getByText("Result B")).toBeTruthy();
  });

  it("calls source with debounced query on input", async () => {
    const source = vi.fn((_q: string) => Promise.resolve<SelectOption[]>([]));

    render(() => <Select source={source} placeholder="Search" />);
    screen.getByText("Search").click();

    vi.advanceTimersByTime(300);

    const input = document.querySelector('input[placeholder="Search..."]') as HTMLInputElement;
    input.value = "abc";
    input.dispatchEvent(new Event("input", { bubbles: true }));

    vi.advanceTimersByTime(300);
    await Promise.resolve();

    expect(source).toHaveBeenCalledWith("abc");
  });
});