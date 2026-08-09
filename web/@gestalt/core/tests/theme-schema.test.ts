import { describe, expect, it, beforeAll } from "vitest";
import Ajv from "ajv/dist/2020";
import type { ValidateFunction } from "ajv/dist/2020";
import schema from "../schemas/theme-schema.json";
import { themes, type ThemeKey } from "../src/design/themes";
import type { ThemeTokens } from "../src/design/tokens";

function color(value: string) {
  return { $type: "color", $value: value };
}

function shadow(colorHex: string, blurPx: number, offsetYPx: number) {
  return {
    $type: "shadow",
    $value: {
      offsetX: { value: 0, unit: "px" },
      offsetY: { value: offsetYPx, unit: "px" },
      blur: { value: blurPx, unit: "px" },
      spread: { value: 0, unit: "px" },
      color: colorHex,
    },
  };
}

const semanticRoles = ["primary", "secondary", "info", "success", "warning", "danger"] as const;
const states = ["", "Hover", "Active", "Focus", "Disabled"] as const;

function makeSemanticColors(): Record<string, unknown> {
  const c: Record<string, unknown> = {};
  for (const role of semanticRoles) {
    for (const state of states) {
      const suffix = state;
      c[`${role}${suffix}`] = color("#aabbcc");
      c[`${role}${suffix}Content`] = color("#ffffff");
    }
  }
  return c;
}

function makeStructuralColors(): Record<string, unknown> {
  return {
    surface: color("#000000"),
    surfaceContent: color("#f0f0f0"),
    surfaceAlt: color("#111111"),
    surfaceAltContent: color("#f0f0f0"),
    highEmphasis: color("#f0f0f0"),
    mediumEmphasis: color("#888888"),
    lowEmphasis: color("#555555"),
    border: color("#1a1a1a"),
    overlay: color("#00000080"),
  };
}

function validTheme() {
  return {
    name: "Test Theme",
    colors: {
      ...makeSemanticColors(),
      ...makeStructuralColors(),
    },
    typography: {
      fontFamily: { $type: "fontFamily", $value: ["Geist Mono", "monospace"] },
    },
    shadows: {
      shadowXs: shadow("#0000000d", 1, 1),
      shadowSm: shadow("#0000001a", 2, 1),
      shadowMd: shadow("#00000026", 8, 4),
      shadowLg: shadow("#00000033", 16, 8),
      shadowXl: shadow("#0000004d", 24, 12),
    },
    borders: {
      borderWidth: { $type: "dimension", $value: { value: 1, unit: "px" } },
    },
  };
}

describe("theme-schema", () => {
  let validate: ValidateFunction;

  beforeAll(() => {
    const ajv = new Ajv({ allErrors: true });
    validate = ajv.compile(schema);
  });

  it("validates a complete 76-token theme", () => {
    const valid = validate(validTheme());
    expect(validate.errors).toBeNull();
    expect(valid).toBe(true);
  });

  it("rejects missing required color token", () => {
    const theme = validTheme();
    delete (theme.colors as Record<string, unknown>)["primaryActive"];
    const valid = validate(theme);
    expect(valid).toBe(false);
    const missing = validate.errors!.flatMap((e) =>
      e.keyword === "required" && e.params && "missingProperty" in e.params
        ? [(e.params as Record<string, string>).missingProperty]
        : []
    );
    expect(missing).toContain("primaryActive");
  });

  it("rejects wrong $type on a color token", () => {
    const theme = validTheme();
    (theme.colors as Record<string, unknown>)["primary"] = { $type: "dimension", $value: "10px" };
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  it("rejects invalid hex color", () => {
    const theme = validTheme();
    (theme.colors as Record<string, unknown>)["surface"] = color("#GGGGGG");
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  it("rejects invalid font key", () => {
    const theme = validTheme();
    theme.typography.fontFamily = { $type: "fontFamily", $value: ["Comic Sans", "cursive"] };
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  it("rejects invalid border width unit", () => {
    const theme = validTheme();
    theme.borders.borderWidth = { $type: "dimension", $value: { value: 1, unit: "em" as const } };
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  it("rejects extra unknown property at top level", () => {
    const theme = { ...validTheme(), extraField: "nope" };
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  it("rejects missing entire category", () => {
    const theme = validTheme();
    delete (theme as Record<string, unknown>)["shadows"];
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  it("accepts channel-based color format", () => {
    const theme = validTheme();
    (theme.colors as Record<string, unknown>)["surface"] = {
      $type: "color",
      $value: { colorSpace: "srgb", components: [0, 0, 0], alpha: 1 },
    };
    const valid = validate(theme);
    expect(valid).toBe(true);
  });

  it("rejects missing name", () => {
    const theme = validTheme();
    delete (theme as Record<string, unknown>)["name"];
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  it("rejects name exceeding max length", () => {
    const theme = validTheme();
    theme.name = "x".repeat(65);
    const valid = validate(theme);
    expect(valid).toBe(false);
  });

  describe("built-in themes", () => {
    const stateMappings = [
      ["default", ""],
      ["defaultContent", "Content"],
      ["hover", "Hover"],
      ["hoverContent", "HoverContent"],
      ["active", "Active"],
      ["activeContent", "ActiveContent"],
      ["focus", "Focus"],
      ["focusContent", "FocusContent"],
      ["disabled", "Disabled"],
      ["disabledContent", "DisabledContent"],
    ] as const;

    function themeToDctg(theme: ThemeTokens): unknown {
      const colors: Record<string, unknown> = {};
      const structKeys = ["surface", "surfaceContent", "surfaceAlt", "surfaceAltContent", "highEmphasis", "mediumEmphasis", "lowEmphasis", "border", "overlay"] as const;
      for (const key of structKeys) {
        const value = (theme.colors as Record<string, unknown>)[key];
        if (typeof value === "string") {
          colors[key] = color(value);
        }
      }
      const roles = ["primary", "secondary", "info", "success", "warning", "danger"] as const;
      for (const role of roles) {
        const states = (theme.colors as Record<string, unknown>)[role] as Record<string, string> | undefined;
        if (!states) continue;
        for (const [stateKey, stateSuffix] of stateMappings) {
          const value = states[stateKey];
          if (typeof value === "string") {
            colors[`${role}${stateSuffix}`] = color(value);
          }
        }
      }
      return {
        name: theme.name,
        description: theme.description,
        author: theme.author,
        colors,
        typography: theme.typography,
        shadows: theme.shadows,
        borders: theme.borders,
      };
    }

    const themeKeys = Object.keys(themes) as ThemeKey[];
    for (const key of themeKeys) {
      it(`validates built-in theme "${key}"`, () => {
        const dctg = themeToDctg(themes[key]);
        const valid = validate(dctg);
        if (!valid) {
          console.error(JSON.stringify(validate.errors, null, 2));
        }
        expect(valid).toBe(true);
      });
    }
  });
});