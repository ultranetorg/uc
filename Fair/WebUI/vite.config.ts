import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import tsconfigPaths from "vite-tsconfig-paths"
import { viteSingleFile } from "vite-plugin-singlefile"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tsconfigPaths(), viteSingleFile()],
  build: {
    target: "esnext",
    assetsInlineLimit: 100_000_000,
    cssCodeSplit: false,
  },
})
