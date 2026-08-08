import plugin from "tailwindcss/plugin";
import { themes } from "./themes";

function camelToKebab(str: string): string {
  return str.replace(/([A-Z])/g, "-$1").toLowerCase();
}

export default plugin(({ addBase }) => {
  const baseStyles: Record<string, Record<string, string>> = {};

  for (const [themeKey, themeConfig] of Object.entries(themes)) {
    const cssVars: Record<string, string> = {};

    for (const [tokenKey, value] of Object.entries(themeConfig.colors)) {
      const cssVar = `--theme-color-${camelToKebab(tokenKey)}`;
      cssVars[cssVar] = value;
    }

    baseStyles[`[data-theme="${themeKey}"]`] = cssVars;
  }

  addBase(baseStyles);
});
