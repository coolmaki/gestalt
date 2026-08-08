import { render } from "solid-js/web";
import "@/styles.css";
import { ThemeProvider } from "@/theme-provider";
import { App } from "./App";

render(
  () => (
    <ThemeProvider defaultTheme="obsidian">
      <App />
    </ThemeProvider>
  ),
  document.getElementById("app")!,
);