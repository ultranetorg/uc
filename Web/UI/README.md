# Web.UI

Common component library for Ultranet web projects.

## Ho to develop

Run the next command:

```bash
yarn dev
```

## How to use

Go to Web.UI folder and run:

```bash
yarn link
yarn build-watch
```

Then go to your application folder and run:

```bash
yarn link 'web.ui'
```

After that you will be able to import Web.UI components:

```ts
import { CircleButton } from "web.ui"
```

## Links

General:

- [https://habr.com/ru/articles/754878/](https://habr.com/ru/articles/754878/)

Create React Typescript library in Vite:

- [https://dev.to/receter/how-to-create-a-react-component-library-using-vites-library-mode-4lma](https://dev.to/receter/how-to-create-a-react-component-library-using-vites-library-mode-4lma)
- [https://medium.com/@mevlutcantuna/building-a-modern-react-component-library-a-guide-with-vite-typescript-and-tailwind-css-862558516b8d](https://medium.com/@mevlutcantuna/building-a-modern-react-component-library-a-guide-with-vite-typescript-and-tailwind-css-862558516b8d)

Prettier configuration:

- [https://prettier.io/docs/en/editors](https://prettier.io/docs/en/editors)
- [https://github.com/prettier/prettier-vscode](https://github.com/prettier/prettier-vscode)
- [https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode)
