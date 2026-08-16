import { writeFileSync, mkdirSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { tokensToCssVars } from "../src/design/token-css-vars";
import { themes } from "../src/design/themes";

const __dirname = dirname(fileURLToPath(import.meta.url));
const themesDir = resolve(__dirname, "..", "src", "themes");

mkdirSync(themesDir, { recursive: true });

for (const [key, theme] of Object.entries(themes)) {
  const vars = tokensToCssVars(theme);
  const lines = Object.entries(vars).map(([name, value]) => `  ${name}: ${value};`);
  const css = `[data-theme="${key}"] {\n${lines.join("\n")}\n}\n`;
  const filePath = resolve(themesDir, `${key}.css`);
  writeFileSync(filePath, css, "utf-8");
  console.log(`Wrote ${key}.css (${lines.length} vars)`);
}