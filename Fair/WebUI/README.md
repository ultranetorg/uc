# Fair.WebUI

## Prerequisites

- [Node.js](https://nodejs.org/) >= 22.18.0 (needed to run the TypeScript sync script natively; tested on 25.1.0)
- npm (bundled with Node.js)

## Installation

1. Go to the `WebUI` directory — the project root, where `package.json` is located:

   ```bash
   cd Fair/WebUI
   ```

2. Install dependencies:

   ```bash
   npm install
   ```

3. Create a `.env` file in the project root and set the API URL (see [Environment variables](#environment-variables)):

   ```env
   VITE_APP_API_BASE_URL=http://127.1.0.100:1080/api
   ```

4. Run the project (see [Development](#development) below).

## Development

Starts a local dev server with hot reload (Vite):

```bash
npm run dev
```

## Build

Standard production build (node sync + type-check + Vite build), output goes to `dist/`. The [sync](#sync-with-node) runs automatically before the build and stops it on failure:

```bash
npm run build
```

Serverless build (single-file, bundled build for static/serverless hosting; requires `yarn` to be available on PATH):

```bash
npm run build:serverless
```

Preview a production build locally:

```bash
npm run preview
```

## Sync with node

[tools/sync-with-node.ts](tools/sync-with-node.ts) copies node-side (C#) constants into the WebUI sources:

| Source                          | Target                                                                          |
| ------------------------------- | ------------------------------------------------------------------------------- |
| `Fair/cs/FairNode.cs` `WelcomeMessage` | `<li>` items between `@sync-start/@sync-end` markers in `ParadigmDescription.tsx` |
| `Net/Mcv/Operation.cs` error constants | constants marked with `// @sync Operation.<Name>` in `src/utils/errorUtils.ts`   |

```bash
npm run sync        # update the targets (also runs automatically before `npm run build`)
npm run sync:check  # only report out-of-sync targets, exit code 1 if any
```

On any problem (missing file, marker or constant, unsupported C# syntax) the script prints `SYNC FAILED` with the reason and exits with code 1, which also stops the build. Edit the text in the C# files, not in the marked WebUI blocks — they are overwritten.

## Environment variables

Configuration is read from a `.env` file in the project root (see [Vite env docs](https://vite.dev/guide/env-and-mode)). Only variables prefixed with `VITE_APP_` are exposed to the client code.

| Variable                       | Description                                                                             |
| ------------------------------ | --------------------------------------------------------------------------------------- |
| `VITE_APP_API_BASE_URL`        | Base URL of the Fair API the app talks to.                                              |
| `VITE_APP_ICCP_NODE_TEST_URL`  | (Optional) URL of an ICCP test node. Commented out by default.                          |
| `VITE_APP_SERVERLESS_BUILD`    | Set automatically by `npm run build:serverless`; switches on the serverless build mode. |

Example `.env`:

```env
VITE_APP_API_BASE_URL=http://127.1.0.100:1080/api
#VITE_APP_ICCP_NODE_TEST_URL=http://127.1.0.100:3160
```

### RAMDISK_DIR (optional)

Optional build-time flag read by `vite.config.ts` from the **process environment** (it is not read from `.env`, so set it in the shell before running Vite). When set, Vite's cache directory is moved to `<RAMDISK_DIR>/vite-cache/<project folder name>` instead of the default `node_modules/.vite`. Pointing it at a RAM disk speeds up dev server start and rebuilds and reduces disk wear. When unset, the default cache location is used.

```bash
RAMDISK_DIR=/mnt/ramdisk npm run dev
```

## Links

General:

- [https://habr.com/ru/articles/754878/](https://habr.com/ru/articles/754878/)
