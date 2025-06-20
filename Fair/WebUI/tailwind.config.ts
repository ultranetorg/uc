import type { Config } from "tailwindcss"

export default {
  content: ["./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      animation: {
        "spin-slow": "spin 3s linear infinite",
      },
      boxShadow: {
        md: "0 4px 14px 0 rgba(28, 38, 58, 0.1)",
      },
      colors: {
        favorite: "#e3a427",
        dark: {
          100: "#14181f",
        },
        gray: {
          0: "#ffffff",
          50: "#fcfcfd",
          75: "#fefefe",
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
      fontFamily: {
        sans: ["Inter", "sans-serif"],
      },
      fontSize: {
        "2xs": "0.8125rem",
        "2sm": "0.9375rem",
        "2base": "1.0625rem",
        "3.5xl": "2rem",
      },
      letterSpacing: {
        "tight-048": "0.48px",
      },
      lineHeight: {
        "3.5": "0.875rem",
        "3.75": "0.9375rem",
        "4.25": "1.0625rem",
        "4.5": "1.125rem",
        "5.25": "1.3125rem",
        "9.75": "2.4375rem",
      },
      screens: {
        xs: "475px",
      },
      spacing: {
        "2.5": "0.625rem",
        "5.25": "1.3125rem",
        "8.5": "2.125rem",
        13: "3.25rem",
        18: "4.5rem",
        "21.5": "5.375rem",
        39: "9.75rem",
        "42.5": "10.625rem",
        51: "12.75rem",
        55: "13.75rem",
        61: "15.25rem",
        65: "16.25rem",
        "67.5": "16.875rem",
        "67.75": "16.9375rem",
        "87.75": "21.9375rem",
        "91.25": "22.8125rem",
        "106.5": "26.625rem",
        "111.5": "27.875rem",
      },
    },
  },
} satisfies Config
