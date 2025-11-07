import type { Meta, StoryObj } from "@storybook/react"

import { Tooltip } from "./Tooltip"

const meta = {
  title: "Tooltip",
  component: Tooltip,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: {
    text: "Lorem Ipsum is simply dummy text of the printing.",
  },
} satisfies Meta<typeof Tooltip>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = { args: { children: "Default Tooltip" } }

export const TopPlace: Story = { args: { children: "TopPlace Tooltip", place: "top" } }

export const OpenOnClick: Story = { args: { children: "OpenOnClick Tooltip", openOnClick: true } }
