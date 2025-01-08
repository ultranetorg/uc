import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import dts from "vite-plugin-dts"
import { resolve } from "path"
import tsconfigPaths from "vite-tsconfig-paths"

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tsconfigPaths(),
    dts({ tsconfigPath: "./tsconfig.app.json", exclude: ["**/*.stories.ts", "**/assets"], rollupTypes: true }),
  ],
  build: {
    copyPublicDir: false,
    lib: {
      entry: resolve(__dirname, "src/index.ts"),
      name: "web.ui",
      formats: ["es"],
    },
    rollupOptions: {
      external: [
        "lodash",
        "react-device-detect",
        "react-dom",
        "react-paginate",
        "react-router-dom",
        "react-select",
        "react-tooltip",
        "react",
        "react/jsx-runtime",
        "tailwind-merge",
        "tailwindcss",
        "usehooks-ts",
      ],
    },
  },
})
