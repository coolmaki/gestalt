import { describe, it, expect, beforeEach } from "vitest";
import { ThemeProvider, useTheme } from "@/theme-provider";
import { render, screen } from "@solidjs/testing-library";

function TestConsumer() {
  const ctx = useTheme();
  return <div data-testid="theme">{ctx.themeKey}</div>;
}

function ThemeSwitcher() {
  const ctx = useTheme();
  return (
    <div>
      <span data-testid="current">{ctx.themeKey}</span>
      <button data-testid="switch-matrix" onClick={() => ctx.setTheme("matrix")}>
        Matrix
      </button>
      <button data-testid="switch-vapor" onClick={() => ctx.setTheme("vapor")}>
        Vapor
      </button>
    </div>
  );
}

describe("ThemeProvider", () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute("data-theme");
  });

  it("sets data-theme on html element on mount", () => {
    render(() => (
      <ThemeProvider>
        <TestConsumer />
      </ThemeProvider>
    ));

    expect(document.documentElement.getAttribute("data-theme")).toBe("obsidian");
  });

  it("persists theme to localStorage on change", () => {
    render(() => (
      <ThemeProvider>
        <ThemeSwitcher />
      </ThemeProvider>
    ));

    screen.getByTestId("switch-matrix").click();

    expect(localStorage.getItem("supercluster-theme")).toBe("matrix");
    expect(document.documentElement.getAttribute("data-theme")).toBe("matrix");
  });

  it("uses defaultTheme prop when no localStorage value exists", () => {
    render(() => (
      <ThemeProvider defaultTheme="pearl">
        <TestConsumer />
      </ThemeProvider>
    ));

    expect(document.documentElement.getAttribute("data-theme")).toBe("pearl");
  });

  it("restores theme from localStorage on mount", () => {
    localStorage.setItem("supercluster-theme", "vapor");

    render(() => (
      <ThemeProvider>
        <TestConsumer />
      </ThemeProvider>
    ));

    expect(document.documentElement.getAttribute("data-theme")).toBe("vapor");
  });

  it("switches theme and updates data-theme attribute", () => {
    render(() => (
      <ThemeProvider>
        <ThemeSwitcher />
      </ThemeProvider>
    ));

    expect(screen.getByTestId("current").textContent).toBe("obsidian");

    screen.getByTestId("switch-vapor").click();

    expect(localStorage.getItem("supercluster-theme")).toBe("vapor");
    expect(document.documentElement.getAttribute("data-theme")).toBe("vapor");
  });
});