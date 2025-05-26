import type { Config } from "tailwindcss"

export default {
  content: ["./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      animation: {
        "spin-slow": "spin 3s linear infinite",
      },
      colors: {
        favorite: "#e3a427",
        dark: {
          100: "#14181f",
        },
        gray: {
          0: "#ffffff",
          50: "#fcfcfd",
          200: "#e8e9f1",
          100: "#f3f4f9",
          300: "#d2d4e4",
          400: "#9798a6",
          500: "#737582",
          700: "#3f3f46",
          800: "#2a2932",
          950: "#0c0e22",
        },
      },
      fontSize: {
        "2xs": "0.8125rem",
        "2sm": "0.9375rem",
      },
      letterSpacing: {
        "tight-048": "0.48px",
      },
      lineHeight: {
        "3.5": "0.875rem",
        "3.75": "0.9375rem",
        "4.5": "1.125rem",
      },
      screens: {
        xs: "475px",
      },
      spacing: {
        "2.5": "0.625rem",
        18: "4.5rem",
        51: "12.75rem",
        55: "13.75rem",
        61: "15.25rem",
        65: "16.25rem",
      },
    },
  },
} satisfies Config
