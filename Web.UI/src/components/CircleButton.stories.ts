import type { Meta, StoryObj } from "@storybook/react"
import { fn } from "@storybook/test"

import { CircleButton } from "./CircleButton"

const meta = {
  title: "CircleButton",
  component: CircleButton,
  parameters: {
    layout: "centered",
  },
  args: { onClick: fn() },
} satisfies Meta<typeof CircleButton>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: {},
}
