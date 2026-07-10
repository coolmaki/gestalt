import { resolve } from "node:path";
import { defineConfig } from "vite";
import solid from "vite-plugin-solid";
import dts from "vite-plugin-dts";
// import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [
    solid(),
    dts({
      include: ["src"],
      outDir: "dist",
      rollupTypes: true,
    }),
    // tailwindcss(),
  ],
  resolve: {
    alias: {
      "@": resolve(import.meta.dirname, "src"),
    },
  },
  build: {
    lib: {
      entry: resolve(import.meta.dirname, "src/index.ts"),
      formats: ["es"],
      fileName: () => "index.js",
      cssFileName: "styles",
    },
    rollupOptions: {
      external: ["solid-js", /^solid-js\//],
    },
  },
});
