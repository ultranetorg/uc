# Fair.WebUI

## Prerequisites

- [Node.js](https://nodejs.org/) >= 20.0.0 (latest LTS recommended, tested on 25.1.0)
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

   If installation fails with dependency errors, retry with:

   ```bash
   npm install --force
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

Standard production build (type-check + Vite build), output goes to `dist/`:

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

## Environment variables

Configuration is read from a `.env` file in the project root (see [Vite env docs](https://vite.dev/guide/env-and-mode)). Only variables prefixed with `VITE_APP_` are exposed to the client code.

| Variable                        | Description                                                                          |
| -------------------------------- | ------------------------------------------------------------------------------------- |
| `VITE_APP_API_BASE_URL`          | Base URL of the Fair API the app talks to.                                            |
| `VITE_APP_ICCP_NODE_TEST_URL`    | (Optional) URL of an ICCP test node. Commented out by default.                        |
| `VITE_APP_SERVERLESS_BUILD`      | Set automatically by `npm run build:serverless`; switches on the serverless build mode. |

Example `.env`:

```env
VITE_APP_API_BASE_URL=http://127.1.0.100:1080/api
#VITE_APP_ICCP_NODE_TEST_URL=http://127.1.0.100:3160
```

## Links

General:

- [https://habr.com/ru/articles/754878/](https://habr.com/ru/articles/754878/)
