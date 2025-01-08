import type { Meta, StoryObj } from "@storybook/react"

import { TagLinksList } from "./TagLinksList"

const meta = {
  title: "TagLinksList",
  component: TagLinksList,
  parameters: {
    layout: "centered",
  },
} satisfies Meta<typeof TagLinksList>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: {
    items: [
      { label: "Google", to: "#" },
      { label: "Bing", to: "#" },
      { label: "DuckDuckGo", to: "#" },
    ],
  },
}
