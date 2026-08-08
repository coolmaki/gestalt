import type { ColorTokens } from "./tokens";

export interface ThemeConfig {
  name: string;
  colors: ColorTokens;
}

export const obsidian: ThemeConfig = {
  name: "Obsidian",
  colors: {
    surface: "#000000",
    surfaceContent: "#f0f0f0",
    surfaceAlt: "#0d0d0d",
    surfaceAltContent: "#f0f0f0",
    primary: "#a78bfa",
    primaryContent: "#ffffff",
    primaryHover: "#c4b5fd",
    primaryHoverContent: "#ffffff",
    info: "#93c5fd",
    infoContent: "#ffffff",
    success: "#6ee7b7",
    successContent: "#ffffff",
    warning: "#fcd34d",
    warningContent: "#ffffff",
    danger: "#fca5a5",
    dangerContent: "#ffffff",
    dangerHover: "#fecaca",
    dangerHoverContent: "#ffffff",
    highEmphasis: "#f0f0f0",
    mediumEmphasis: "#888888",
    lowEmphasis: "#555555",
    border: "#1a1a1a",
    disabled: "#1a1a1a",
    disabledContent: "#555555",
  },
};

export const matrix: ThemeConfig = {
  name: "Matrix",
  colors: {
    surface: "#000000",
    surfaceContent: "#f0f0f0",
    surfaceAlt: "#0d0d0d",
    surfaceAltContent: "#f0f0f0",
    primary: "#00ff88",
    primaryContent: "#ffffff",
    primaryHover: "#33ffaa",
    primaryHoverContent: "#ffffff",
    info: "#00ccff",
    infoContent: "#ffffff",
    success: "#00ff66",
    successContent: "#ffffff",
    warning: "#ffaa00",
    warningContent: "#ffffff",
    danger: "#ff3344",
    dangerContent: "#ffffff",
    dangerHover: "#ff5566",
    dangerHoverContent: "#ffffff",
    highEmphasis: "#f0f0f0",
    mediumEmphasis: "#888888",
    lowEmphasis: "#555555",
    border: "#1a1a1a",
    disabled: "#1a1a1a",
    disabledContent: "#555555",
  },
};

export const pearl: ThemeConfig = {
  name: "Pearl",
  colors: {
    surface: "#ffffff",
    surfaceContent: "#111111",
    surfaceAlt: "#f5f5f5",
    surfaceAltContent: "#111111",
    primary: "#818cf8",
    primaryContent: "#ffffff",
    primaryHover: "#a5b4fc",
    primaryHoverContent: "#ffffff",
    info: "#60a5fa",
    infoContent: "#ffffff",
    success: "#34d399",
    successContent: "#ffffff",
    warning: "#f59e0b",
    warningContent: "#ffffff",
    danger: "#ef4444",
    dangerContent: "#ffffff",
    dangerHover: "#f87171",
    dangerHoverContent: "#ffffff",
    highEmphasis: "#111111",
    mediumEmphasis: "#666666",
    lowEmphasis: "#999999",
    border: "#e5e5e5",
    disabled: "#e5e5e5",
    disabledContent: "#999999",
  },
};

export const vapor: ThemeConfig = {
  name: "Vapor",
  colors: {
    surface: "#ffffff",
    surfaceContent: "#111111",
    surfaceAlt: "#f5f5f5",
    surfaceAltContent: "#111111",
    primary: "#00ddff",
    primaryContent: "#ffffff",
    primaryHover: "#33eeff",
    primaryHoverContent: "#ffffff",
    info: "#3399ff",
    infoContent: "#ffffff",
    success: "#00dd66",
    successContent: "#ffffff",
    warning: "#ff7700",
    warningContent: "#ffffff",
    danger: "#ff2266",
    dangerContent: "#ffffff",
    dangerHover: "#ff5588",
    dangerHoverContent: "#ffffff",
    highEmphasis: "#111111",
    mediumEmphasis: "#666666",
    lowEmphasis: "#999999",
    border: "#e5e5e5",
    disabled: "#e5e5e5",
    disabledContent: "#999999",
  },
};

export type ThemeKey = "obsidian" | "matrix" | "pearl" | "vapor";

export const themes: Record<ThemeKey, ThemeConfig> = {
  "obsidian": obsidian,
  "matrix": matrix,
  "pearl": pearl,
  "vapor": vapor,
};

export const availableThemes = Object.keys(themes) as ThemeKey[];
