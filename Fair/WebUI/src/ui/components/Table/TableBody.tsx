import { memo } from "react"
import { get } from "lodash"
import { twMerge } from "tailwind-merge"

import { TableProps } from "./types"
import { TableRow } from "./TableRow"

export type TableBodyProps = Pick<TableProps, "columns" | "emptyState" | "items" | "itemRenderer">

export const TableBody = memo(({ columns, emptyState, items, itemRenderer }: TableBodyProps) =>
  items && items.length > 0 ? (
    items.map(item => (
      <TableRow key={item.id}>
        {columns.map(column => {
          const value = get(item, column.accessor)
          const renderedValue = itemRenderer && itemRenderer(item, column)
          return (
            <div className={column.className ?? "flex-1"} key={`${item.id}_${column.accessor}`}>
              {renderedValue ?? value?.toString?.() ?? ""}
            </div>
          )
        })}
      </TableRow>
    ))
  ) : (
    <TableRow className={twMerge(emptyState && "py-6")}>{emptyState ? emptyState : null}</TableRow>
  ),
)
