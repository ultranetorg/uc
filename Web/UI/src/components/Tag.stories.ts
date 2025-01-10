import type { Meta, StoryObj } from "@storybook/react"

import { Tag } from "./Tag"

const meta = {
  title: "Tag",
  component: Tag,
  parameters: {
    layout: "centered",
  },
  args: {},
} satisfies Meta<typeof Tag>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: { label: "Default Tag" },
}

export const WithTooltip: Story = {
  args: { label: "Tooltip Tag", tooltipText: "Hello World!" },
}
