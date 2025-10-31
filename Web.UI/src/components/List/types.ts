import { ReactNode } from "react"

import { Breakpoints } from "constants/"

export type ListItems = {
  [key: string]: any
}

export type ListRow = {
  accessor: string
  label?: string | null
  description?: string | null
  type?: string
  extra?: boolean
  hint?: string
}

export type ListItemRenderer = (value: any, row: ListRow, items: ListItems, breakpoint?: Breakpoints) => ReactNode
