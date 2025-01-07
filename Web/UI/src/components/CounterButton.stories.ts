import type { Meta, StoryObj } from "@storybook/react"
import { fn } from "@storybook/test"

import { CounterButton } from "./CounterButton"

const meta = {
  title: "CounterButton",
  component: CounterButton,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: { onClick: fn() },
} satisfies Meta<typeof CounterButton>

export default meta
type Story = StoryObj<typeof meta>

export const Primary: Story = {
  args: { count: 45, counterLabel: "likes", buttonLabel: "👍" },
}
