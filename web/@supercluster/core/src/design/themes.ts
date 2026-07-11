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
    primaryContent: "#0d0d0d",
    primaryHover: "#c4b5fd",
    primaryHoverContent: "#0d0d0d",
    info: "#93c5fd",
    infoContent: "#0d0d0d",
    success: "#6ee7b7",
    successContent: "#0d0d0d",
    warning: "#fcd34d",
    warningContent: "#0d0d0d",
    danger: "#fca5a5",
    dangerContent: "#0d0d0d",
    dangerHover: "#fecaca",
    dangerHoverContent: "#0d0d0d",
    highEmphasis: "#f0f0f0",
    mediumEmphasis: "#888888",
    lowEmphasis: "#555555",
    border: "#1a1a1a",
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
    primaryContent: "#0d0d0d",
    primaryHover: "#33ffaa",
    primaryHoverContent: "#0d0d0d",
    info: "#00ccff",
    infoContent: "#0d0d0d",
    success: "#00ff66",
    successContent: "#0d0d0d",
    warning: "#ffaa00",
    warningContent: "#0d0d0d",
    danger: "#ff3344",
    dangerContent: "#0d0d0d",
    dangerHover: "#ff5566",
    dangerHoverContent: "#0d0d0d",
    highEmphasis: "#f0f0f0",
    mediumEmphasis: "#888888",
    lowEmphasis: "#555555",
    border: "#1a1a1a",
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
    primaryContent: "#111111",
    primaryHover: "#33eeff",
    primaryHoverContent: "#111111",
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
