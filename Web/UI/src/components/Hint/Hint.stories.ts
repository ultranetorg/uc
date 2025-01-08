import type { Meta, StoryObj } from "@storybook/react"

import { Hint } from "./Hint"

const meta = {
  title: "Hint",
  component: Hint,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: {
    title: "Lorem Ipsum",
    text: "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
  },
} satisfies Meta<typeof Hint>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {}
