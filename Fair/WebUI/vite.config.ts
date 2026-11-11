import path from "path"
import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import { viteSingleFile } from "vite-plugin-singlefile"
import svgr from "vite-plugin-svgr"

const ramdiskDir = process.env.RAMDISK_DIR

export default defineConfig({
  plugins: [react(), svgr({ svgrOptions: { ref: true } }), viteSingleFile()],

  ...(ramdiskDir
    ? {
        cacheDir: path.resolve(ramdiskDir, "vite-cache", path.basename(process.cwd())),
      }
    : {}),

  build: {
    target: "esnext",
    assetsInlineLimit: 100_000_000,
    cssCodeSplit: false,
  },

  resolve: {
    alias: {
      api: path.resolve(import.meta.dirname, "src/api"),
      app: path.resolve(import.meta.dirname, "src/app"),
      assets: path.resolve(import.meta.dirname, "src/assets"),
      constants: path.resolve(import.meta.dirname, "src/constants"),
      entities: path.resolve(import.meta.dirname, "src/entities"),
      hooks: path.resolve(import.meta.dirname, "src/hooks"),
      types: path.resolve(import.meta.dirname, "src/types"),
      ui: path.resolve(import.meta.dirname, "src/ui"),
      utils: path.resolve(import.meta.dirname, "src/utils"),
      config: path.resolve(import.meta.dirname, "src/config.ts"),
      testConfig: path.resolve(import.meta.dirname, "src/testConfig.tsx"),
    },
  },
})
