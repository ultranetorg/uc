import { ReactNode, memo } from "react"
import { get } from "lodash"
import { twMerge } from "tailwind-merge"

import { Breakpoints } from "constants"

import { TableItemRenderer, TableColumn, TableItem } from "./types"

export type TableBodyProps = {
  columns: TableColumn[]
  items: TableItem[]
  noRowsOverlay?: ReactNode
  breakpoint?: Breakpoints
  itemRenderer?: (column: TableColumn, item: TableItem) => TableItemRenderer
}

export const TableBody = memo((props: TableBodyProps) => {
  const { columns, items, noRowsOverlay, breakpoint, itemRenderer } = props

  return (
    <tbody>
      <tr className="h-5" />
      {!!noRowsOverlay && items.length === 0 ? (
        <tr>
          <td colSpan={columns.length}>{noRowsOverlay}</td>
        </tr>
      ) : (
        <>
          {items.map(item => {
            return (
              <tr className="h-10" key={item.id}>
                {columns.map(column => {
                  const value = get(item, column.accessor)
                  const renderer = itemRenderer && itemRenderer(column, item)

                  return (
                    <td
                      key={column.accessor + item.id}
                      className={twMerge("overflow-hidden text-ellipsis whitespace-nowrap px-4", column.className)}
                    >
                      {renderer ? renderer(value, item, column, breakpoint) : value}
                    </td>
                  )
                })}
              </tr>
            )
          })}
        </>
      )}
      <tr className="h-5" />
    </tbody>
  )
})