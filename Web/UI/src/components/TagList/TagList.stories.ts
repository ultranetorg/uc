import type { Meta, StoryObj } from "@storybook/react"

import { TagsList } from "./TagsList"

const meta = {
  title: "TagsList",
  component: TagsList,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: {
    expandLabel: "Expand",
    collapseLabel: "Collapse",
  },
} satisfies Meta<typeof TagsList>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: {
    items: [
      { label: "Windows", tooltipText: "Microsoft Windows" },
      { label: "Word", tooltipText: "Microsoft Word" },
      { label: "Paint", tooltipText: "Paint" },
      { label: "Edge", tooltipText: "Microsoft Edge" },
      { label: "Outlook", tooltipText: "Microsoft Outlook" },
    ],
    collapsedItemsCount: 3,
  },
}
