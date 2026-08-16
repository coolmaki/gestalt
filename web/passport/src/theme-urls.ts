import obsidianUrl from "@gestalt/core/src/themes/obsidian.css?url";
import pearlUrl from "@gestalt/core/src/themes/pearl.css?url";
import matrixUrl from "@gestalt/core/src/themes/matrix.css?url";
import vaporUrl from "@gestalt/core/src/themes/vapor.css?url";

if (typeof window !== "undefined") {
  window.__gestaltThemeUrls = {
    obsidian: obsidianUrl,
    pearl: pearlUrl,
    matrix: matrixUrl,
    vapor: vaporUrl,
  };
}