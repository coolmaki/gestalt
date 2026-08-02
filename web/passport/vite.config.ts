import { defineConfig } from "vite";
import solid from "vite-plugin-solid";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [tailwindcss(), solid()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      "/api": "http://localhost:5000",
      "/.well-known": "http://localhost:5000",
    },
  },
  build: {
    outDir: "../../src/Passport/wwwroot",
    emptyOutDir: true,
  },
});