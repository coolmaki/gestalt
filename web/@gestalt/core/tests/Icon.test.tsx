import { describe, it, expect } from "vitest";
import { render, waitFor } from "@solidjs/testing-library";
import { Icon } from "@/components/Icon";

describe("Icon", () => {
  it("renders a tick icon", async () => {
    render(() => <Icon name="tick" />);
    await waitFor(() => {
      expect(document.querySelector("svg")).toBeTruthy();
    });
  });

  it("renders with custom size", async () => {
    render(() => <Icon name="cross" size={32} />);
    await waitFor(() => {
      const el = document.querySelector("svg");
      expect(el).toBeTruthy();
      expect(el?.getAttribute("width")).toBe("32");
      expect(el?.getAttribute("height")).toBe("32");
    });
  });

  it("applies custom class", async () => {
    render(() => <Icon name="search" class="text-primary" />);
    await waitFor(() => {
      const el = document.querySelector("span");
      expect(el).toBeTruthy();
      expect(el?.classList.contains("text-primary")).toBe(true);
    });
  });
});