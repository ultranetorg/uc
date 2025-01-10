import type { Meta, StoryObj } from "@storybook/react"
import { fn } from "@storybook/test"

import { ContentLink } from "./ContentLink"

const meta = {
  title: "ContentLink",
  component: ContentLink,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: { onClick: fn() },
} satisfies Meta<typeof ContentLink>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: { children: "Default ContentLink", to: "#" },
}

export const External: Story = {
  args: { children: "External ContentLink", to: "#", showIcon: true, external: true },
}

export const Internal: Story = {
  args: { children: "Internal ContentLink", to: "#", showIcon: true, external: false },
}
