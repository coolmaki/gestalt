import { describe, it, expect } from "vitest";
import { render, waitFor } from "@solidjs/testing-library";
import { LoadingIndicator } from "@/components/LoadingIndicator";

describe("LoadingIndicator", () => {
  it("renders a spinner with animate-spin", async () => {
    render(() => <LoadingIndicator />);
    await waitFor(() => {
      expect(document.querySelector("svg")).toBeTruthy();
    });
    const wrapper = document.querySelector("span");
    expect(wrapper?.classList.contains("animate-spin")).toBe(true);
  });

  it("uses 24 as default size", async () => {
    render(() => <LoadingIndicator />);
    await waitFor(() => {
      expect(document.querySelector("svg")).toBeTruthy();
    });
    const svg = document.querySelector("svg");
    expect(svg?.getAttribute("width")).toBe("24");
    expect(svg?.getAttribute("height")).toBe("24");
  });

  it("respects the size prop", async () => {
    render(() => <LoadingIndicator size={16} />);
    await waitFor(() => {
      expect(document.querySelector("svg")).toBeTruthy();
    });
    const svg = document.querySelector("svg");
    expect(svg?.getAttribute("width")).toBe("16");
    expect(svg?.getAttribute("height")).toBe("16");
  });

  it("applies custom class", async () => {
    render(() => <LoadingIndicator class="text-primary" />);
    await waitFor(() => {
      expect(document.querySelector("span")).toBeTruthy();
    });
    const wrapper = document.querySelector("span");
    expect(wrapper?.classList.contains("text-primary")).toBe(true);
  });
});