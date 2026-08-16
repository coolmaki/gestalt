import { createContext, createMemo, createSignal, useContext, type Accessor, type JSX } from "solid-js";
import type { ThemeKey } from "./design/themes";

declare global {
  interface Window {
    __gestaltThemeUrls?: Record<ThemeKey, string>;
  }
}

const THEME_COOKIE = "gestalt-theme";
const THEME_STORAGE_KEY = "gestalt-theme";
const RADIUS_STORAGE_KEY = "gestalt-radius";
const LINK_ID = "gestalt-theme-css";
const DEFAULT_THEME: ThemeKey = "obsidian";

export type Radius = "none" | "sm" | "md" | "lg";

export interface ThemeMeta {
  key: ThemeKey;
  name: string;
}

export const themeMetaList: ThemeMeta[] = [
  { key: "obsidian", name: "Obsidian" },
  { key: "matrix", name: "Matrix" },
  { key: "pearl", name: "Pearl" },
  { key: "vapor", name: "Vapor" },
];

interface ThemeContextValue {
  themeKey: Accessor<ThemeKey>;
  setTheme: (key: ThemeKey) => void;
  availableThemes: ThemeMeta[];
  radius: Accessor<Radius>;
  setRadius: (r: Radius) => void;
}

const ThemeContext = createContext<ThemeContextValue>();

export interface ThemeProviderProps {
  children: JSX.Element;
  defaultTheme?: ThemeKey;
  defaultRadius?: Radius;
}

function readCookie(key: string): string | undefined {
  if (typeof document === "undefined") return undefined;
  try {
    const m = document.cookie.match(new RegExp(`(?:^|;\\s*)${key}=([^;]*)`));
    return m ? decodeURIComponent(m[1]) : undefined;
  } catch {
    return undefined;
  }
}

function setCookie(key: string, value: string): void {
  if (typeof document === "undefined") return;
  document.cookie = `${key}=${encodeURIComponent(value)}; path=/; SameSite=Lax; max-age=31536000`;
}

function applyTheme(key: ThemeKey): void {
  document.documentElement.setAttribute("data-theme", key);
}

function applyRadius(r: Radius): void {
  document.documentElement.setAttribute("data-radius", r);
}

function swapThemeCss(key: ThemeKey): void {
  const existing = document.getElementById(LINK_ID) as HTMLLinkElement | null;
  if (existing) existing.remove();

  if (key === DEFAULT_THEME) {
    return;
  }

  const url = window.__gestaltThemeUrls?.[key];
  if (!url) return;

  const link = document.createElement("link");
  link.id = LINK_ID;
  link.rel = "stylesheet";
  link.href = url;
  document.head.appendChild(link);
}

function getInitialThemeKey(): ThemeKey {
  if (typeof window === "undefined") return DEFAULT_THEME;

  const cookieVal = readCookie(THEME_COOKIE);
  if (cookieVal && ["obsidian", "matrix", "pearl", "vapor"].includes(cookieVal)) {
    return cookieVal as ThemeKey;
  }

  try {
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    if (stored && ["obsidian", "matrix", "pearl", "vapor"].includes(stored)) {
      return stored as ThemeKey;
    }
  } catch { /* empty */ }

  return DEFAULT_THEME;
}

export function ThemeProvider(props: ThemeProviderProps) {
  const initialKey: ThemeKey = props.defaultTheme ?? getInitialThemeKey();

  const [themeKey, setThemeKey] = createSignal<ThemeKey>(initialKey);
  const initialRadius: Radius = (() => {
    try {
      return (localStorage.getItem(RADIUS_STORAGE_KEY) as Radius | null) ?? props.defaultRadius ?? "sm";
    } catch {
      return props.defaultRadius ?? "sm";
    }
  })();
  const [radius, setRadiusSignal] = createSignal<Radius>(initialRadius);

  applyTheme(initialKey);
  applyRadius(initialRadius);

  swapThemeCss(initialKey);

  if (typeof window !== "undefined") {
    window.addEventListener("storage", (event) => {
      if (event.key === THEME_STORAGE_KEY && event.newValue) {
        const validKeys = ["obsidian", "matrix", "pearl", "vapor"];
        if (validKeys.includes(event.newValue)) {
          const newKey = event.newValue as ThemeKey;
          setThemeKey(newKey);
          applyTheme(newKey);
          swapThemeCss(newKey);
        }
      }
      if (event.key === RADIUS_STORAGE_KEY && event.newValue) {
        setRadiusSignal(event.newValue as Radius);
        applyRadius(event.newValue as Radius);
      }
    });
  }

  const setTheme = (key: ThemeKey) => {
    setThemeKey(key);
    setCookie(THEME_COOKIE, key);
    try {
      localStorage.setItem(THEME_STORAGE_KEY, key);
    } catch { /* empty */ }
    applyTheme(key);
    swapThemeCss(key);
  };

  const setRadius = (r: Radius) => {
    setRadiusSignal(r);
    try {
      localStorage.setItem(RADIUS_STORAGE_KEY, r);
    } catch { /* empty */ }
    applyRadius(r);
  };

  return (
    <ThemeContext.Provider value={{
      themeKey: themeKey,
      setTheme,
      availableThemes: themeMetaList,
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