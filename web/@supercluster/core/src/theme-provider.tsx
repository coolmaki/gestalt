import { createSignal, createContext, useContext, type JSX } from "solid-js";
import { type ThemeConfig, type ThemeKey, themes, availableThemes } from "./design/themes";

const THEME_STORAGE_KEY = "supercluster-theme";

interface ThemeContextValue {
  theme: ThemeConfig;
  themeKey: ThemeKey;
  setTheme: (key: ThemeKey) => void;
  availableThemes: ThemeConfig[];
}

const ThemeContext = createContext<ThemeContextValue>();

export interface ThemeProviderProps {
  children: JSX.Element;
  defaultTheme?: ThemeKey;
}

function applyTheme(key: ThemeKey): void {
  document.documentElement.setAttribute("data-theme", key);
}

export function ThemeProvider(props: ThemeProviderProps) {
  const stored = localStorage.getItem(THEME_STORAGE_KEY) as ThemeKey | null;
  const initialKey: ThemeKey = stored && stored in themes
    ? stored
    : (props.defaultTheme ?? "obsidian");

  const [themeKey, setThemeKey] = createSignal<ThemeKey>(initialKey);

  const themeList = availableThemes.map((key) => themes[key]);

  applyTheme(initialKey);

  if (typeof window !== "undefined") {
    window.addEventListener("storage", (event) => {
      if (
        event.key === THEME_STORAGE_KEY &&
        event.newValue &&
        event.newValue in themes
      ) {
        const newKey = event.newValue as ThemeKey;
        setThemeKey(newKey);
        applyTheme(newKey);
      }
    });
  }

  const setTheme = (key: ThemeKey) => {
    setThemeKey(key);
    localStorage.setItem(THEME_STORAGE_KEY, key);
    applyTheme(key);
  };

  return (
    <ThemeContext.Provider
      value={{
        theme: themes[themeKey()],
        themeKey: themeKey(),
        setTheme,
        availableThemes: themeList,
      }}
    >
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
