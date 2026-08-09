import type { ColorTokens, ShadowTokens, BorderTokens, TypographyTokens, ThemeTokens } from "./tokens";
import { deriveColorStates } from "./color-utils";

function shadows(isDark: boolean): ShadowTokens {
  const alpha = isDark ? ["0d", "1a", "26", "33", "4d"] : ["08", "10", "14", "1a", "26"];
  return {
    shadowXs: { $type: "shadow", $value: { offsetX: { value: 0, unit: "px" }, offsetY: { value: 1, unit: "px" }, blur: { value: 1, unit: "px" }, spread: { value: 0, unit: "px" }, color: `#000000${alpha[0]}` } },
    shadowSm: { $type: "shadow", $value: { offsetX: { value: 0, unit: "px" }, offsetY: { value: 1, unit: "px" }, blur: { value: 2, unit: "px" }, spread: { value: 0, unit: "px" }, color: `#000000${alpha[1]}` } },
    shadowMd: { $type: "shadow", $value: { offsetX: { value: 0, unit: "px" }, offsetY: { value: 4, unit: "px" }, blur: { value: 8, unit: "px" }, spread: { value: 0, unit: "px" }, color: `#000000${alpha[2]}` } },
    shadowLg: { $type: "shadow", $value: { offsetX: { value: 0, unit: "px" }, offsetY: { value: 8, unit: "px" }, blur: { value: 16, unit: "px" }, spread: { value: 0, unit: "px" }, color: `#000000${alpha[3]}` } },
    shadowXl: { $type: "shadow", $value: { offsetX: { value: 0, unit: "px" }, offsetY: { value: 12, unit: "px" }, blur: { value: 24, unit: "px" }, spread: { value: 0, unit: "px" }, color: `#000000${alpha[4]}` } },
  };
}

const borderDefault: BorderTokens = {
  borderWidth: { $type: "dimension", $value: { value: 1, unit: "px" } },
};

const fontDefault: TypographyTokens = {
  fontFamily: { $type: "fontFamily", $value: ["Geist Mono", "monospace"] },
};

function struct(isDark: boolean): { surface: string; surfaceContent: string; surfaceAlt: string; surfaceAltContent: string; highEmphasis: string; mediumEmphasis: string; lowEmphasis: string; border: string; overlay: string } {
  return isDark
    ? {
        surface: "#000000",
        surfaceContent: "#f0f0f0",
        surfaceAlt: "#0d0d0d",
        surfaceAltContent: "#f0f0f0",
        highEmphasis: "#f0f0f0",
        mediumEmphasis: "#888888",
        lowEmphasis: "#555555",
        border: "#1a1a1a",
        overlay: "#00000080",
      }
    : {
        surface: "#ffffff",
        surfaceContent: "#111111",
        surfaceAlt: "#f5f5f5",
        surfaceAltContent: "#111111",
        highEmphasis: "#111111",
        mediumEmphasis: "#666666",
        lowEmphasis: "#999999",
        border: "#e5e5e5",
        overlay: "#00000033",
      };
}

export type ThemeKey = "obsidian" | "matrix" | "pearl" | "vapor";

export const obsidian: ThemeTokens = {
  name: "Obsidian",
  colors: {
    ...struct(true),
    primary: deriveColorStates("#a78bfa", "#ffffff", true, { hover: "#c4b5fd" }),
    secondary: deriveColorStates("#4c4f6b", "#ffffff", true),
    info: deriveColorStates("#93c5fd", "#ffffff", true),
    success: deriveColorStates("#6ee7b7", "#ffffff", true),
    warning: deriveColorStates("#fcd34d", "#ffffff", true),
    danger: deriveColorStates("#fca5a5", "#ffffff", true, { hover: "#fecaca" }),
  },
  typography: fontDefault,
  shadows: shadows(true),
  borders: borderDefault,
};

export const matrix: ThemeTokens = {
  name: "Matrix",
  colors: {
    ...struct(true),
    primary: deriveColorStates("#00ff88", "#ffffff", true, { hover: "#33ffaa" }),
    secondary: deriveColorStates("#1a4a3a", "#ffffff", true),
    info: deriveColorStates("#00ccff", "#ffffff", true),
    success: deriveColorStates("#00ff66", "#ffffff", true),
    warning: deriveColorStates("#ffaa00", "#ffffff", true),
    danger: deriveColorStates("#ff3344", "#ffffff", true, { hover: "#ff5566" }),
  },
  typography: fontDefault,
  shadows: shadows(true),
  borders: borderDefault,
};

export const pearl: ThemeTokens = {
  name: "Pearl",
  colors: {
    ...struct(false),
    primary: deriveColorStates("#818cf8", "#ffffff", false, { hover: "#a5b4fc" }),
    secondary: deriveColorStates("#8b8b8b", "#ffffff", false),
    info: deriveColorStates("#60a5fa", "#ffffff", false),
    success: deriveColorStates("#34d399", "#ffffff", false),
    warning: deriveColorStates("#f59e0b", "#ffffff", false),
    danger: deriveColorStates("#ef4444", "#ffffff", false, { hover: "#f87171" }),
  },
  typography: fontDefault,
  shadows: shadows(false),
  borders: borderDefault,
};

export const vapor: ThemeTokens = {
  name: "Vapor",
  colors: {
    ...struct(false),
    primary: deriveColorStates("#00ddff", "#ffffff", false, { hover: "#33eeff" }),
    secondary: deriveColorStates("#664455", "#ffffff", false),
    info: deriveColorStates("#3399ff", "#ffffff", false),
    success: deriveColorStates("#00dd66", "#ffffff", false),
    warning: deriveColorStates("#ff7700", "#ffffff", false),
    danger: deriveColorStates("#ff2266", "#ffffff", false, { hover: "#ff5588" }),
  },
  typography: fontDefault,
  shadows: shadows(false),
  borders: borderDefault,
};

export const themes: Record<ThemeKey, ThemeTokens> = {
  obsidian,
  matrix,
  pearl,
  vapor,
};

export const availableThemes = Object.keys(themes) as ThemeKey[];