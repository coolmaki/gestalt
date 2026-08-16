import { render } from "solid-js/web";
import "@/styles.css";
import { ThemeProvider } from "@/theme-provider";
import { App } from "./App";

import obsidianUrl from "../src/themes/obsidian.css?url";
import matrixUrl from "../src/themes/matrix.css?url";
import pearlUrl from "../src/themes/pearl.css?url";
import vaporUrl from "../src/themes/vapor.css?url";

if (typeof window !== "undefined") {
  window.__gestaltThemeUrls = {
    obsidian: obsidianUrl,
    matrix: matrixUrl,
    pearl: pearlUrl,
    vapor: vaporUrl,
  };
}

render(
  () => (
    <ThemeProvider defaultTheme="obsidian">
      <App />
    </ThemeProvider>
  ),
  document.getElementById("app")!,
);