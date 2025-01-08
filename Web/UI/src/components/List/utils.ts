import { get } from "lodash"

import { Breakpoints } from "constants/"

import { ListItemRenderer, ListItems, ListRow } from "./types"
import { ReactNode } from "react"

export const renderValue = (
  items: ListItems,
  row: ListRow,
  itemRenderer?: (row: ListRow, items: ListItems) => ListItemRenderer,
  breakpoint?: Breakpoints,
): ReactNode => {
  const value = get(items, row.accessor)
  if (value === null || value === undefined) {
    return undefined
  }

  const renderer = itemRenderer && itemRenderer(row, value)
  const valueToRender = renderer ? renderer(value, row, items, breakpoint) : value
  return valueToRender !== null && valueToRender !== undefined ? valueToRender : undefined
}
