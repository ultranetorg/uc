import type { Meta, StoryObj } from "@storybook/react"
import { fn } from "@storybook/test"

import { TableFooter } from "./TableFooter"

const meta = {
  title: "TableFooter",
  component: TableFooter,
  parameters: {
    layout: "centered",
  },
  args: {
    className: "gap-6",
  },
} satisfies Meta<typeof TableFooter>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: { page: 1, pageSize: 10, totalItems: 100, onPageChange: fn() },
}
