// --- Neutral Scale (backgrounds, borders, emphasis) ---

export const neutral = {
  "0": "#000000",
  "50": "#0a0a0a",
  "100": "#111111",
  "200": "#1a1a1a",
  "300": "#2a2a2a",
  "400": "#3a3a3a",
  "500": "#555555",
  "600": "#717171",
  "700": "#888888",
  "800": "#999999",
  "900": "#b3b3b3",
  "950": "#cccccc",
  "1000": "#e5e5e5",
  "1100": "#f0f0f0",
  "1150": "#f5f5f5",
  "1200": "#ffffff",
} as const;

// --- Accent Hue Scales (for semantic color derivation) ---

export const violet = {
  "200": "#1e1b4b",
  "300": "#312e81",
  "400": "#4338ca",
  "500": "#6366f1",
  "600": "#818cf8",
  "700": "#a5b4fc",
  "800": "#c4b5fd",
  "900": "#ddd6fe",
} as const;

export const cyan = {
  "200": "#002233",
  "300": "#003344",
  "400": "#0077aa",
  "500": "#00aacc",
  "600": "#33ccff",
  "700": "#66ddff",
  "800": "#99eeff",
} as const;

export const emerald = {
  "200": "#002211",
  "300": "#003311",
  "400": "#007744",
  "500": "#00aa66",
  "600": "#33cc88",
  "700": "#66ddaa",
  "800": "#99eecc",
} as const;

export const amber = {
  "200": "#332200",
  "300": "#553300",
  "400": "#995500",
  "500": "#cc7700",
  "600": "#ee9900",
  "700": "#ffbb33",
  "800": "#ffdd77",
} as const;

export const rose = {
  "200": "#330011",
  "300": "#550022",
  "400": "#992244",
  "500": "#cc3344",
  "600": "#ee5566",
  "700": "#ff7788",
  "800": "#ff99aa",
} as const;

// --- Semantic Hue Mappings ---

export const accents = {
  primary: violet,
  secondary: amber,
  info: cyan,
  success: emerald,
  warning: amber,
  danger: rose,
} as const;

// --- Type Scale (global — not user-themable) ---

export const typeScale = {
  xs: {
    fontSize: "0.75rem",
    lineHeight: "1rem",
    fontWeight: "400",
  },
  sm: {
    fontSize: "0.875rem",
    lineHeight: "1.25rem",
    fontWeight: "400",
  },
  base: {
    fontSize: "1rem",
    lineHeight: "1.5rem",
    fontWeight: "400",
  },
  lg: {
    fontSize: "1.125rem",
    lineHeight: "1.75rem",
    fontWeight: "500",
  },
  xl: {
    fontSize: "1.25rem",
    lineHeight: "1.75rem",
    fontWeight: "600",
  },
  "2xl": {
    fontSize: "1.5rem",
    lineHeight: "2rem",
    fontWeight: "600",
  },
  "3xl": {
    fontSize: "1.875rem",
    lineHeight: "2.25rem",
    fontWeight: "700",
  },
} as const;

// --- Spacing Scale (global — not user-themable) ---

export const spacing = {
  "0": "0rem",
  "1": "0.25rem",
  "2": "0.5rem",
  "3": "0.75rem",
  "4": "1rem",
  "5": "1.25rem",
  "6": "1.5rem",
  "8": "2rem",
  "10": "2.5rem",
  "12": "3rem",
  "16": "4rem",
  "20": "5rem",
  "24": "6rem",
  "32": "8rem",
} as const;