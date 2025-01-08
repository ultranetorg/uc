import type { Meta, StoryObj } from "@storybook/react"
import { fn } from "@storybook/test"

import { OvalButton } from "./OvalButton"

const meta = {
  title: "OvalButton",
  component: OvalButton,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: { onClick: fn() },
} satisfies Meta<typeof OvalButton>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: { label: "Button" },
}
