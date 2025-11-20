import js from "@eslint/js"
import globals from "globals"
import reactHooks from "eslint-plugin-react-hooks"
import reactRefresh from "eslint-plugin-react-refresh"
import tseslint from "typescript-eslint"
import eslintPluginPrettier from "eslint-plugin-prettier"
import tailwindcss from "eslint-plugin-tailwindcss"
import eslintConfigPrettier from "eslint-config-prettier"
import importPlugin from "eslint-plugin-import"

export default tseslint.config(
  { ignores: ["dist"] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ["**/*.{ts,tsx}"],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
      tailwindcss,
      // register under the "import" namespace so rules like "import/no-unresolved" work
      import: importPlugin,
      prettier: eslintPluginPrettier,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      // prettier
      ...eslintConfigPrettier.rules,
      ...eslintPluginPrettier.configs.recommended.rules,
      // tailwind
      ...tailwindcss.configs.recommended.rules,
      // ensure the prettier rule is enabled
      "prettier/prettier": "error",
      "react-refresh/only-export-components": ["warn", { allowConstantExport: true }],
      // import plugin
      ...importPlugin.configs.recommended.rules,
      "import/order": ["warn", { groups: ["builtin", "external", "internal", "parent", "sibling", "index"] }],
    },
    settings: {
      "import/resolver": {
        typescript: {},
      },
    },
    ignores: ["node_modules", "dist", "coverage"],
  },
)
