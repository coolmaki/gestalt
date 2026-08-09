import plugin from "tailwindcss/plugin";
import { themes } from "./themes";
import type { ColorTokens, ColorStateTokens, ShadowTokens, BorderTokens, TypographyTokens } from "./tokens";

function camelToKebab(str: string): string {
  return str.replace(/([A-Z])/g, "-$1").toLowerCase();
}

function flattenColors(colors: ColorTokens): Record<string, string> {
  const vars: Record<string, string> = {};

  const structuralKeys = [
    "surface", "surfaceContent", "surfaceAlt", "surfaceAltContent",
    "highEmphasis", "mediumEmphasis", "lowEmphasis", "border", "overlay",
  ] as const;
  for (const key of structuralKeys) {
    if (colors[key] !== undefined) {
      vars[`--theme-color-${camelToKebab(key)}`] = colors[key] as string;
    }
  }

  const semanticRoles = ["primary", "secondary", "info", "success", "warning", "danger"] as const;
  const stateMappings: Array<[keyof ColorStateTokens, string]> = [
    ["default", ""],
    ["defaultContent", "-content"],
    ["hover", "-hover"],
    ["hoverContent", "-hover-content"],
    ["active", "-active"],
    ["activeContent", "-active-content"],
    ["focus", "-focus"],
    ["focusContent", "-focus-content"],
    ["disabled", "-disabled"],
    ["disabledContent", "-disabled-content"],
  ];

  for (const role of semanticRoles) {
    const states = colors[role];
    if (!states) continue;
    for (const [stateKey, stateSuffix] of stateMappings) {
      const value = states[stateKey];
      if (value !== undefined) {
        vars[`--theme-color-${camelToKebab(role)}${stateSuffix}`] = value;
      }
    }
  }

  return vars;
}

function flattenShadows(shadows: ShadowTokens): Record<string, string> {
  const vars: Record<string, string> = {};
  for (const [key, shadow] of Object.entries(shadows)) {
    const sv = shadow.$value;
    vars[`--theme-${camelToKebab(key)}`] =
      `${sv.offsetX.value}${sv.offsetX.unit} ${sv.offsetY.value}${sv.offsetY.unit} ${sv.blur.value}${sv.blur.unit} ${sv.spread.value}${sv.spread.unit} ${sv.color}`;
  }
  return vars;
}

function flattenTypography(typography: TypographyTokens): Record<string, string> {
  return {
    "--theme-font-family": typography.fontFamily.$value.join(", "),
  };
}

function flattenBorders(borders: BorderTokens): Record<string, string> {
  const bv = borders.borderWidth.$value;
  return {
    "--theme-border-width": `${bv.value}${bv.unit}`,
  };
}

export default plugin(({ addBase }) => {
  const baseStyles: Record<string, Record<string, string>> = {};

  for (const [themeKey, themeConfig] of Object.entries(themes)) {
    baseStyles[`[data-theme="${themeKey}"]`] = {
      ...flattenColors(themeConfig.colors),
      ...flattenShadows(themeConfig.shadows),
      ...flattenTypography(themeConfig.typography),
      ...flattenBorders(themeConfig.borders),
    };
  }

  addBase(baseStyles);
});