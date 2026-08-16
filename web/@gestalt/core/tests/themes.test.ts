import { describe, it, expect } from "vitest";
import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { themes, type ThemeKey } from "../src/design/themes";
import { tokensToCssVars } from "../src/design/token-css-vars";

const HEX_PATTERN = /^#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?$/;
const DIMENSION_PATTERN = /^(px|rem)$/;

const SEMANTIC_ROLES = ["primary", "secondary", "info", "success", "warning", "danger"] as const;
const STATES = ["default", "hover", "active", "focus", "disabled"] as const;
const STRUCTURAL_KEYS = [
  "surface",
  "surfaceContent",
  "surfaceAlt",
  "surfaceAltContent",
  "highEmphasis",
  "mediumEmphasis",
  "lowEmphasis",
  "border",
  "overlay",
] as const;
const SHADOW_KEYS = ["shadowXs", "shadowSm", "shadowMd", "shadowLg", "shadowXl"] as const;

const themeKeys = Object.keys(themes) as ThemeKey[];

describe("built-in themes", () => {
  it("defines all 4 built-in themes", () => {
    expect(themeKeys.sort()).toEqual(["matrix", "obsidian", "pearl", "vapor"]);
  });

  for (const key of themeKeys) {
    const theme = themes[key];

    describe(key, () => {
      it("has 69 valid color tokens", () => {
        for (const role of SEMANTIC_ROLES) {
          for (const state of STATES) {
            expect(theme.colors[role][state], `${role}.${state}`).toMatch(HEX_PATTERN);
            expect(theme.colors[role][`${state}Content`], `${role}.${state}Content`).toMatch(HEX_PATTERN);
          }
        }
        for (const structural of STRUCTURAL_KEYS) {
          expect(theme.colors[structural], structural).toMatch(HEX_PATTERN);
        }
      });

      it("has 5 shadow tokens with valid dimensions", () => {
        for (const shadowKey of SHADOW_KEYS) {
          const shadow = theme.shadows[shadowKey].$value;
          expect(shadow.offsetX.unit, `${shadowKey}.offsetX.unit`).toMatch(DIMENSION_PATTERN);
          expect(shadow.offsetY.unit, `${shadowKey}.offsetY.unit`).toMatch(DIMENSION_PATTERN);
          expect(shadow.blur.unit, `${shadowKey}.blur.unit`).toMatch(DIMENSION_PATTERN);
          expect(shadow.spread.unit, `${shadowKey}.spread.unit`).toMatch(DIMENSION_PATTERN);
          expect(typeof shadow.offsetX.value, `${shadowKey}.offsetX.value`).toBe("number");
          expect(typeof shadow.offsetY.value, `${shadowKey}.offsetY.value`).toBe("number");
          expect(typeof shadow.blur.value, `${shadowKey}.blur.value`).toBe("number");
          expect(typeof shadow.spread.value, `${shadowKey}.spread.value`).toBe("number");
          expect(shadow.color, `${shadowKey}.color`).toMatch(HEX_PATTERN);
        }
      });

      it("has a valid font family", () => {
        const families = theme.typography.fontFamily.$value;
        expect(families.length).toBeGreaterThan(0);
        for (const family of families) {
          expect(family).toBeTruthy();
        }
      });

      it("has a valid border width", () => {
        const border = theme.borders.borderWidth.$value;
        expect(typeof border.value).toBe("number");
        expect(border.unit).toMatch(DIMENSION_PATTERN);
      });
    });
  }
});

describe("tokensToCssVars", () => {
  it("produces exactly 76 CSS vars for every theme", () => {
    for (const key of themeKeys) {
      const vars = tokensToCssVars(themes[key]);
      expect(Object.keys(vars).length, `${key} var count`).toBe(76);
    }
  });

  it("produces the same var names across all themes", () => {
    const namesByTheme = themeKeys.map((key) =>
      Object.keys(tokensToCssVars(themes[key])).sort(),
    );
    for (let i = 1; i < namesByTheme.length; i++) {
      expect(namesByTheme[i]).toEqual(namesByTheme[0]);
    }
  });

  it("uses kebab-case names prefixed with --theme-", () => {
    for (const key of themeKeys) {
      for (const name of Object.keys(tokensToCssVars(themes[key]))) {
        expect(name.startsWith("--theme-"), name).toBe(true);
        expect(name, name).not.toMatch(/[A-Z]/);
      }
    }
  });

  it("assigns 50% alpha to focus color states", () => {
    for (const key of themeKeys) {
      const vars = tokensToCssVars(themes[key]);
      for (const role of SEMANTIC_ROLES) {
        const focusVar = vars[`--theme-color-${role}-focus`];
        expect(focusVar, `${key}.${role}.focus`).toMatch(/#[0-9a-fA-F]{6}80$/);
      }
    }
  });
});

describe("generated theme CSS", () => {
  it("matches tokensToCssVars output for every theme", () => {
    for (const key of themeKeys) {
      const cssPath = resolve(import.meta.dirname, "..", "src", "themes", `${key}.css`);
      const css = readFileSync(cssPath, "utf-8");

      expect(css, `${key} selector`).toContain(`[data-theme="${key}"]`);

      const vars = tokensToCssVars(themes[key]);
      for (const [name, value] of Object.entries(vars)) {
        const declaration = `${name}: ${value};`;
        expect(css, `${key}: missing ${declaration}`).toContain(declaration);
      }
    }
  });
});
