import type { Meta, StoryObj } from "@storybook/react"

import { Loader } from "./Loader"

const meta = {
  title: "Loader",
  component: Loader,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
} satisfies Meta<typeof Loader>

export default meta
type Story = StoryObj<typeof meta>

export const Large: Story = {
  args: { size: "large" },
}

export const ExtraLarge: Story = {
  args: { size: "x-large" },
}
