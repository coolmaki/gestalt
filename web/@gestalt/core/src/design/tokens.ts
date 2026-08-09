export interface HexColorValue {
  $type: "color";
  $value: string;
}

export interface ChannelColorValue {
  $type: "color";
  $value: {
    colorSpace: string;
    components: number[];
    alpha?: number;
  };
}

export type ColorValue = HexColorValue | ChannelColorValue;

export interface DimensionValue {
  $type: "dimension";
  $value: {
    value: number;
    unit: "px" | "rem";
  };
}

export interface ShadowValue {
  $type: "shadow";
  $value: {
    offsetX: { value: number; unit: "px" | "rem" };
    offsetY: { value: number; unit: "px" | "rem" };
    blur: { value: number; unit: "px" | "rem" };
    spread: { value: number; unit: "px" | "rem" };
    color: string;
  };
}

export interface FontFamilyValue {
  $type: "fontFamily";
  $value: string[];
}

export type FontKey = "geist-mono" | "jetbrains-mono" | "fira-code" | "ibm-plex-mono" | "system-mono";

export interface SemanticColorTokens {
  primary: ColorStateTokens;
  secondary: ColorStateTokens;
  info: ColorStateTokens;
  success: ColorStateTokens;
  warning: ColorStateTokens;
  danger: ColorStateTokens;
}

export interface ColorStateTokens {
  default: string;
  defaultContent: string;
  hover: string;
  hoverContent: string;
  active: string;
  activeContent: string;
  focus: string;
  focusContent: string;
  disabled: string;
  disabledContent: string;
}

export interface StructuralColorTokens {
  surface: string;
  surfaceContent: string;
  surfaceAlt: string;
  surfaceAltContent: string;
  highEmphasis: string;
  mediumEmphasis: string;
  lowEmphasis: string;
  border: string;
  overlay: string;
}

export interface ColorTokens extends StructuralColorTokens, SemanticColorTokens {}

export interface ShadowTokens {
  shadowXs: ShadowValue;
  shadowSm: ShadowValue;
  shadowMd: ShadowValue;
  shadowLg: ShadowValue;
  shadowXl: ShadowValue;
}

export interface BorderTokens {
  borderWidth: DimensionValue;
}

export interface TypographyTokens {
  fontFamily: FontFamilyValue;
}

export interface ThemeTokens {
  name: string;
  description?: string;
  author?: string;
  colors: ColorTokens;
  typography: TypographyTokens;
  shadows: ShadowTokens;
  borders: BorderTokens;
}

