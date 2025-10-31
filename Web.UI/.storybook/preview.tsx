import React from "react";
import type { Preview } from "@storybook/react";
import { MemoryRouter } from "react-router";

import { HintProvider, TooltipProvider, ClickableTooltipProvider } from "../src/components";
import "../src/tailwind.css";

const preview: Preview = {
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
  },
};

export const decorators = [
  (Story) => (
    <MemoryRouter initialEntries={['/']}>
      <HintProvider />
      <TooltipProvider />
      <ClickableTooltipProvider />
      <Story />
    </MemoryRouter>
  ),
];

export default preview;
