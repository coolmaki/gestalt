import { vi } from "vitest";

const storage = new Map<string, string>();

Object.defineProperty(globalThis, "localStorage", {
  value: {
    getItem: (key: string): string | null => storage.get(key) ?? null,
    setItem: (key: string, value: string) => storage.set(key, value),
    removeItem: (key: string) => storage.delete(key),
    clear: () => storage.clear(),
    get length(): number { return storage.size; },
    key: (index: number): string | null => Array.from(storage.keys())[index] ?? null,
  },
  writable: true,
});

vi.mock("@/components/Icon/registry", () => ({
  loadIconShapes: async (_name: string) => [{ type: "path", d: "M0 0h10v10H0z" }],
}));