import path from "path"
import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import { viteSingleFile } from "vite-plugin-singlefile"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), viteSingleFile()],
  build: {
    target: "esnext",
    assetsInlineLimit: 100_000_000,
    cssCodeSplit: false,
  },
  resolve: {
    alias: {
      api: path.resolve(__dirname, "src/api"),
      app: path.resolve(__dirname, "src/app"),
      assets: path.resolve(__dirname, "src/assets"),
      entities: path.resolve(__dirname, "src/entities"),
      hooks: path.resolve(__dirname, "src/hooks"),
      ui: path.resolve(__dirname, "src/ui"),
      utils: path.resolve(__dirname, "src/utils"),
      config: path.resolve(__dirname, "src/config.ts"),
      testConfig: path.resolve(__dirname, "src/testConfig.tsx"),
    },
  },
})
