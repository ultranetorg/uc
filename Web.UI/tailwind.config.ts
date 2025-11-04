import type { Config } from "tailwindcss"

import { tailwindPreset } from "./src/tailwindPreset"

export default {
  content: ["./src/**/*.{ts,tsx}"],
  presets: [tailwindPreset],
} satisfies Config
