import type { Meta, StoryObj } from "@storybook/react"
import { fn } from "@storybook/test"

import { ExpandToggle } from "./ExpandToggle"

const meta = {
  title: "ExpandToggle",
  component: ExpandToggle,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: { onToggle: fn() },
} satisfies Meta<typeof ExpandToggle>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: { expandLabel: "Expand", collapseLabel: "Collapse" },
}
