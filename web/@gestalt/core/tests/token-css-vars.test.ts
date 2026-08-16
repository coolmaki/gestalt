import { describe, it, expect } from "vitest";
import { tokensToCssVars } from "../src/design/token-css-vars";
import { obsidian } from "../src/design/themes";
import type { ThemeTokens } from "../src/design/tokens";

describe("tokensToCssVars", () => {
  it("generates CSS vars for all color categories", () => {
    const vars = tokensToCssVars(obsidian as unknown as ThemeTokens);

    expect(vars["--theme-color-surface"]).toBe("#000000");
    expect(vars["--theme-color-surface-content"]).toBe("#f0f0f0");
    expect(vars["--theme-color-primary"]).toBe("#a78bfa");
    expect(vars["--theme-color-primary-hover"]).toBe("#c4b5fd");
    expect(vars["--theme-color-primary-active"]).toBeDefined();
    expect(vars["--theme-color-primary-focus"]).toBeDefined();
    expect(vars["--theme-color-primary-disabled"]).toBeDefined();
    expect(vars["--theme-color-primary-content"]).toBeDefined();
    expect(vars["--theme-color-secondary"]).toBeDefined();
    expect(vars["--theme-color-overlay"]).toBe("#00000080");
    expect(vars["--theme-color-border"]).toBe("#1a1a1a");
  });

  it("generates shadow CSS vars", () => {
    const vars = tokensToCssVars(obsidian as unknown as ThemeTokens);

    expect(vars["--theme-shadow-xs"]).toContain("px");
    expect(vars["--theme-shadow-xs"]).toContain("#0000000d");
    expect(vars["--theme-shadow-md"]).toContain("8px");
    expect(vars["--theme-shadow-xl"]).toContain("24px");
  });

  it("generates typography CSS var", () => {
    const vars = tokensToCssVars(obsidian as unknown as ThemeTokens);

    expect(vars["--theme-font-family"]).toBe("Geist Mono, monospace");
  });

  it("generates border CSS var", () => {
    const vars = tokensToCssVars(obsidian as unknown as ThemeTokens);

    expect(vars["--theme-border-width"]).toBe("1px");
  });

  it("generates exactly 76 CSS vars", () => {
    const vars = tokensToCssVars(obsidian as unknown as ThemeTokens);

    // 69 colors + 5 shadows + 1 font + 1 border = 76
    expect(Object.keys(vars).length).toBe(76);
  });
});