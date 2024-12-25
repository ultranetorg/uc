import pluginRewriteAll from "vite-plugin-rewrite-all"
import tsconfigPaths from "vite-tsconfig-paths"

import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import svgr from "vite-plugin-svgr"

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), tsconfigPaths(), svgr(), pluginRewriteAll()],
})
