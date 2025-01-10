import type { Meta, StoryObj } from "@storybook/react"

import { TextLink } from "./TextLink"

const meta = {
  title: "TextLink",
  component: TextLink,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: {},
} satisfies Meta<typeof TextLink>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: { text: "TextLink", to: "#" },
}
