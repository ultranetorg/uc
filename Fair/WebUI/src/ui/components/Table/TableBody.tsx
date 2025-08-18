import { memo } from "react"
import { get } from "lodash"
import { twMerge } from "tailwind-merge"

import { TableProps } from "./types"
import { TableRow } from "./TableRow"

export type TableBodyProps = Pick<
  TableProps,
  "columns" | "emptyState" | "items" | "tableBodyClassName" | "itemRenderer" | "onRowClick"
>

export const TableBody = memo(
  ({ columns, emptyState, items, tableBodyClassName, itemRenderer, onRowClick }: TableBodyProps) => (
    <tbody className={tableBodyClassName}>
      {items && items.length > 0 ? (
        items.map(item => (
          <TableRow key={item.id} onClick={() => onRowClick?.(item.id)}>
            {columns.map(column => {
              const value = get(item, column.accessor)
              const renderedValue = itemRenderer && itemRenderer(item, column)
              return (
                <td className={twMerge("px-4 py-3", column.className)} key={`${item.id}_${column.accessor}`}>
                  {renderedValue ?? value?.toString?.() ?? ""}
                </td>
              )
            })}
          </TableRow>
        ))
      ) : (
        <TableRow className={twMerge(emptyState && "py-6")}>
          <td colSpan={columns.length}>{emptyState ? emptyState : null}</td>
        </TableRow>
      )}
    </tbody>
  ),
)
