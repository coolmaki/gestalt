import type { ColorStateTokens } from "./tokens";

function hexToRgb(hex: string): [number, number, number] {
  hex = hex.replace("#", "");
  if (hex.length === 3) {
    hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
  }
  const num = parseInt(hex.substring(0, 6), 16);
  return [(num >> 16) & 255, (num >> 8) & 255, num & 255];
}

function rgbToHex(r: number, g: number, b: number): string {
  const toHex = (n: number) => {
    const clamped = Math.max(0, Math.min(255, Math.round(n)));
    return clamped.toString(16).padStart(2, "0");
  };
  return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
}

function rgbToHsl(r: number, g: number, b: number): [number, number, number] {
  r /= 255;
  g /= 255;
  b /= 255;
  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  let h = 0;
  let s = 0;
  const l = (max + min) / 2;
  if (max !== min) {
    const d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    if (max === r) {
      h = ((g - b) / d + (g < b ? 6 : 0)) % 6;
    } else if (max === g) {
      h = (b - r) / d + 2;
    } else {
      h = (r - g) / d + 4;
    }
    h *= 60;
  }
  return [h, s, l];
}

function hslToRgb(h: number, s: number, l: number): [number, number, number] {
  h = ((h % 360) + 360) % 360;
  s = Math.max(0, Math.min(1, s));
  l = Math.max(0, Math.min(1, l));
  if (s === 0) {
    const v = Math.round(l * 255);
    return [v, v, v];
  }
  const hueToRgb = (p: number, q: number, t: number) => {
    if (t < 0) t += 1;
    if (t > 1) t -= 1;
    if (t < 1 / 6) return p + (q - p) * 6 * t;
    if (t < 1 / 2) return q;
    if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
    return p;
  };
  const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
  const p = 2 * l - q;
  return [
    Math.round(hueToRgb(p, q, h / 360 + 1 / 3) * 255),
    Math.round(hueToRgb(p, q, h / 360) * 255),
    Math.round(hueToRgb(p, q, h / 360 - 1 / 3) * 255),
  ];
}

export function lighten(hex: string, amount: number): string {
  const [r, g, b] = hexToRgb(hex);
  const [h, s, l] = rgbToHsl(r, g, b);
  const [nr, ng, nb] = hslToRgb(h, s, Math.min(1, l + amount));
  return rgbToHex(nr, ng, nb);
}

export function darken(hex: string, amount: number): string {
  const [r, g, b] = hexToRgb(hex);
  const [h, s, l] = rgbToHsl(r, g, b);
  const [nr, ng, nb] = hslToRgb(h, s, Math.max(0, l - amount));
  return rgbToHex(nr, ng, nb);
}

export function desaturate(hex: string, amount: number): string {
  const [r, g, b] = hexToRgb(hex);
  const [h, s, l] = rgbToHsl(r, g, b);
  const [nr, ng, nb] = hslToRgb(h, Math.max(0, s - amount), l);
  return rgbToHex(nr, ng, nb);
}

export function withAlpha(hex: string, alpha: number): string {
  hex = hex.replace("#", "");
  const clamped = Math.max(0, Math.min(1, alpha));
  const alphaHex = Math.round(clamped * 255)
    .toString(16)
    .padStart(2, "0");
  return `#${hex.substring(0, 6)}${alphaHex}`;
}

export function deriveColorStates(
  defaultHex: string,
  contentHex: string,
  isDark: boolean,
  overrides?: Partial<ColorStateTokens>,
): ColorStateTokens {
  const lAmount = isDark ? 0.1 : 0.1;
  const aAmount = isDark ? 0.15 : 0.15;

  const hoverFn = isDark ? lighten : darken;
  const activeFn = isDark ? lighten : darken;

  return {
    default: defaultHex,
    defaultContent: contentHex,
    hover: hoverFn(defaultHex, lAmount),
    hoverContent: contentHex,
    active: activeFn(defaultHex, aAmount),
    activeContent: contentHex,
    focus: withAlpha(defaultHex, 0.5),
    focusContent: contentHex,
    disabled: desaturate(defaultHex, 0.5),
    disabledContent: desaturate(contentHex, 0.3),
    ...overrides,
  };
}