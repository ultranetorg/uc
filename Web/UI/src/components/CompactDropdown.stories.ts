import type { Meta, StoryObj } from "@storybook/react"

import { CompactDropdown } from "./CompactDropdown"

const meta = {
  title: "CompactDropdown",
  component: CompactDropdown,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
} satisfies Meta<typeof CompactDropdown>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: {
    items: [
      { value: "10", label: "10" },
      { value: "20", label: "20" },
      { value: "50", label: "50" },
      { value: "100", label: "100" },
    ],
    value: "50",
  },
}
