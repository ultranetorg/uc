import type { Config } from "tailwindcss"
import defaultTheme from "tailwindcss/defaultTheme"

export default {
  content: ["./src/**/*.{ts,tsx}"],
  theme: {
    colors: {
      gray: {
        100: "#F3F4F9",
        300: "#D2D4E4",
        400: "#9798A6",
        500: "#6B7280",
      },
    },
    screens: {
      xs: "475px",
      ...defaultTheme.screens,
    },
  },
} satisfies Config
