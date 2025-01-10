import type { Meta, StoryObj } from "@storybook/react"

import { CopyableText } from "./CopyableText"

const meta = {
  title: "CopyableText",
  component: CopyableText,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
} satisfies Meta<typeof CopyableText>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: { copiedMessage: "Copied!", text: "Hello World", title: "Click to copy" },
}
