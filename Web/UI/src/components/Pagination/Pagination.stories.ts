import type { Meta, StoryObj } from "@storybook/react"

import { Pagination } from "./Pagination"

const meta = {
  title: "Pagination",
  component: Pagination,
  parameters: {
    layout: "centered",
  },
  argTypes: {},
  args: {},
} satisfies Meta<typeof Pagination>

export default meta
type Story = StoryObj<typeof meta>

export const Primary: Story = {
  args: { page: 0, pagesCount: 36 },
}
