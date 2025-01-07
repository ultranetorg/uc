import type { Meta, StoryObj } from "@storybook/react"
import { fn } from "@storybook/test"

import { BackButton } from "./BackButton"

const meta = {
  title: "BackButton",
  component: BackButton,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: { onClick: fn() },
} satisfies Meta<typeof BackButton>

export default meta
type Story = StoryObj<typeof meta>

export const Primary: Story = {}
