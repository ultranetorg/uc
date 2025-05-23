import type { Config } from "tailwindcss"

export default {
  content: ["./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      animation: {
        "spin-slow": "spin 3s linear infinite",
      },
      colors: {
        favorite: "#E3A427",
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
          700: "#3F3F46",
          800: "#2A2932",
          950: "#0C0E22",
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
