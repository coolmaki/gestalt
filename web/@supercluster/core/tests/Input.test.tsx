import { describe, it, expect } from "vitest";
import { render, screen } from "@solidjs/testing-library";
import { Input } from "@/components/Input";

describe("Input", () => {
  it("renders an input element", () => {
    render(() => <Input placeholder="Enter text" />);
    expect(screen.getByPlaceholderText("Enter text")).toBeTruthy();
  });

  it("renders with a default value", () => {
    render(() => <Input value="hello" />);
    const input = document.querySelector("input");
    expect(input?.value).toBe("hello");
  });

  it("applies error styling", () => {
    render(() => <Input error="Required" />);
    const input = document.querySelector("input");
    expect(input?.classList.contains("border-danger")).toBe(true);
  });

  it("disables input", () => {
    render(() => <Input disabled />);
    const input = document.querySelector("input");
    expect(input?.hasAttribute("disabled")).toBe(true);
  });

  it("calls onChange on input", () => {
    let value = "";
    render(() => <Input onChange={(v) => { value = v; }} />);
    const input = document.querySelector("input") as HTMLInputElement;
    input.value = "test";
    input.dispatchEvent(new Event("input", { bubbles: true }));
    expect(value).toBe("test");
  });
});