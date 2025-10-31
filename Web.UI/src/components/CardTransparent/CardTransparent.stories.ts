import type { Meta, StoryObj } from "@storybook/react"

import { CardTransparent } from "./CardTransparent"

const meta = {
  title: "CardTransparent",
  component: CardTransparent,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
} satisfies Meta<typeof CardTransparent>

export default meta
type Story = StoryObj<typeof meta>

const data = {
  firstName: "John",
  lastName: "Doe",
  age: 33,
}

export const Default: Story = {
  args: {
    title: "CardTransparent",
    rows: [
      { label: "First Name:", accessor: "firstName" },
      { label: "Last Name:", accessor: "lastName" },
      { label: "Age:", accessor: "age" },
    ],
    items: data,
  },
}
