import { createContext, createMemo, createSignal, useContext, type Accessor, type JSX } from "solid-js";
import { availableThemes, themes, type ThemeConfig, type ThemeKey } from "./design/themes";

const THEME_STORAGE_KEY = "supercluster-theme";
const RADIUS_STORAGE_KEY = "supercluster-radius";

export type Radius = "none" | "sm" | "md" | "lg";

interface ThemeContextValue {
  theme: Accessor<ThemeConfig>;
  themeKey: Accessor<ThemeKey>;
  setTheme: (key: ThemeKey) => void;
  availableThemes: ThemeConfig[];
  radius: Accessor<Radius>;
  setRadius: (r: Radius) => void;
}

const ThemeContext = createContext<ThemeContextValue>();

export interface ThemeProviderProps {
  children: JSX.Element;
  defaultTheme?: ThemeKey;
  defaultRadius?: Radius;
}

function applyTheme(key: ThemeKey): void {
  document.documentElement.setAttribute("data-theme", key);
}

function applyRadius(r: Radius): void {
  document.documentElement.setAttribute("data-radius", r);
}

export function ThemeProvider(props: ThemeProviderProps) {
  const stored = localStorage.getItem(THEME_STORAGE_KEY) as ThemeKey | null;
  const initialKey: ThemeKey = stored && stored in themes
    ? stored
    : (props.defaultTheme ?? "obsidian");

  const [themeKey, setThemeKey] = createSignal<ThemeKey>(initialKey);
  const initialRadius: Radius = (localStorage.getItem(RADIUS_STORAGE_KEY) as Radius | null) ?? props.defaultRadius ?? "md";
  const [radius, setRadiusSignal] = createSignal<Radius>(initialRadius);

  const themeList = availableThemes.map((key) => themes[key]);

  applyTheme(initialKey);
  applyRadius(initialRadius);

  if (typeof window !== "undefined") {
    window.addEventListener("storage", (event) => {
      if (event.key === THEME_STORAGE_KEY && event.newValue && event.newValue in themes) {
        const newKey = event.newValue as ThemeKey;
        setThemeKey(newKey);
        applyTheme(newKey);
      }
      if (event.key === RADIUS_STORAGE_KEY && event.newValue) {
        setRadiusSignal(event.newValue as Radius);
        applyRadius(event.newValue as Radius);
      }
    });
  }

  const setTheme = (key: ThemeKey) => {
    setThemeKey(key);
    localStorage.setItem(THEME_STORAGE_KEY, key);
    applyTheme(key);
  };

  const setRadius = (r: Radius) => {
    setRadiusSignal(r);
    localStorage.setItem(RADIUS_STORAGE_KEY, r);
    applyRadius(r);
  };

  const theme = createMemo(() => themes[themeKey()]);

  return (
    <ThemeContext.Provider value={{
      theme: theme,
      themeKey: themeKey,
      setTheme,
      availableThemes: themeList,
      radius: radius,
      setRadius,
    }}>
      {props.children}
    </ThemeContext.Provider>
  );
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) {
    throw new Error("useTheme must be used within a ThemeProvider");
  }
  return ctx;
}