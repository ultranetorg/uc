import type { Meta, StoryObj } from "@storybook/react"

import { Dropdown } from "./Dropdown"

const meta = {
  title: "Dropdown",
  component: Dropdown,
  parameters: {
    layout: "centered",
  },
  // tags: ["autodocs"],
  argTypes: {
    // backgroundColor: { control: 'color' },
  },
  args: {},
} satisfies Meta<typeof Dropdown>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: {
    items: [
      { value: "local", label: "Localzone" },
      { value: "test", label: "Testzone" },
      { value: "dev1", label: "Devzone1" },
      { value: "dev2", label: "Devzone2" },
    ],
    value: "dev1",
  },
}
