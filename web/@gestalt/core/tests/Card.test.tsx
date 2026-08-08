import { describe, it, expect } from "vitest";
import { render, screen } from "@solidjs/testing-library";
import { Card } from "@/components/Card";

describe("Card", () => {
  it("renders children", () => {
    render(() => <Card>Content</Card>);
    expect(screen.getByText("Content")).toBeTruthy();
  });

  it("applies default variant styling", () => {
    render(() => <Card>Content</Card>);
    const card = screen.getByText("Content");
    expect(card.classList.contains("bg-surface-alt")).toBe(true);
    expect(card.classList.contains("border-border")).toBe(true);
  });

  it("applies ghost variant (no surface/border)", () => {
    render(() => <Card variant="ghost">Content</Card>);
    const card = screen.getByText("Content");
    expect(card.classList.contains("bg-surface-alt")).toBe(false);
    expect(card.classList.contains("border-border")).toBe(false);
  });

  it("applies custom class", () => {
    render(() => <Card class="my-class">Content</Card>);
    expect(screen.getByText("Content").classList.contains("my-class")).toBe(true);
  });
});