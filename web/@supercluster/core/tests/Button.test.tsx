import { describe, it, expect } from "vitest";
import { render, screen } from "@solidjs/testing-library";
import { Button } from "@/components/Button";

describe("Button", () => {
  it("renders button text", () => {
    render(() => <Button>Click me</Button>);
    expect(screen.getByText("Click me")).toBeTruthy();
  });

  it("applies primary variant by default", () => {
    render(() => <Button>Default</Button>);
    const btn = screen.getByText("Default");
    expect(btn.classList.contains("bg-primary")).toBe(true);
  });

  it("renders danger variant", () => {
    render(() => <Button variant="danger">Delete</Button>);
    const btn = screen.getByText("Delete");
    expect(btn.classList.contains("bg-danger")).toBe(true);
  });

  it("renders ghost variant", () => {
    render(() => <Button variant="ghost">Ghost</Button>);
    const btn = screen.getByText("Ghost");
    expect(btn.classList.contains("bg-transparent")).toBe(true);
  });

  it("applies secondary variant", () => {
    render(() => <Button variant="secondary">Secondary</Button>);
    const btn = screen.getByText("Secondary");
    expect(btn.classList.contains("bg-surface-alt")).toBe(true);
  });

  it("applies size classes", () => {
    render(() => <Button size="lg">Large</Button>);
    const btn = screen.getByText("Large");
    expect(btn.classList.contains("text-lg")).toBe(true);
  });

  it("disables button when disabled prop is set", () => {
    render(() => <Button disabled>Disabled</Button>);
    const btn = screen.getByText("Disabled");
    expect(btn.hasAttribute("disabled")).toBe(true);
  });

  it("shows loading spinner", () => {
    render(() => <Button loading>Saving</Button>);
    expect(screen.getByText("Saving")).toBeTruthy();
    const svg = document.querySelector("svg");
    expect(svg).toBeTruthy();
  });

  it("calls onClick when clicked", () => {
    let clicked = false;
    render(() => <Button onClick={() => { clicked = true; }}>Click</Button>);
    screen.getByText("Click").click();
    expect(clicked).toBe(true);
  });

  it("does not call onClick when disabled", () => {
    let clicked = false;
    render(() => <Button disabled onClick={() => { clicked = true; }}>Click</Button>);
    screen.getByText("Click").click();
    expect(clicked).toBe(false);
  });
});