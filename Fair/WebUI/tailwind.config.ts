import type { Config } from "tailwindcss"

export default {
  content: ["./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      animation: {
        "spin-slow": "spin 3s linear infinite",
      },
      colors: {
        dark: {
          100: "#14181F",
        },
        gray: {
          50: "#FCFCFD",
          200: "#E8E9F1",
          100: "#F3F4F9",
          300: "#D2D4E4",
          400: "#9798A6",
          500: "#737582",
          800: "#2A2932",
        },
      },
      screens: {
        xs: "475px",
      },
    },
  },
} satisfies Config
