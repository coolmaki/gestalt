import { describe, it, expect } from "vitest";
import { render, screen } from "@solidjs/testing-library";
import { Icon } from "@/components/Icon";

describe("Icon", () => {
  it("renders a check icon", () => {
    render(() => <Icon name="check" />);
    const el = document.querySelector("svg");
    expect(el).toBeTruthy();
  });

  it("renders with custom size", () => {
    render(() => <Icon name="x" size={32} />);
    const el = document.querySelector("svg");
    expect(el?.getAttribute("width")).toBe("32");
    expect(el?.getAttribute("height")).toBe("32");
  });

  it("applies custom class", () => {
    render(() => <Icon name="search" class="text-primary" />);
    const el = document.querySelector("span");
    expect(el?.classList.contains("text-primary")).toBe(true);
  });
});