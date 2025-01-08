import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useBreakpoint } from "hooks"

import { TableHead, TableHeadProps } from "./TableHead"
import { TableBody, TableBodyProps } from "./TableBody"
import { TableFooter, TableFooterProps } from "./TableFooter"

type TableBaseProps = {
  className?: string
}

type TableProps = TableBaseProps &
  TableHeadProps &
  Omit<TableBodyProps, "breakpoint"> &
  Pick<TableFooterProps, "footer">

export const Table = memo((props: TableProps) => {
  const { className, columns, items, itemRenderer, noRowsOverlay, footer } = props

  const breakpoint = useBreakpoint()

  return (
    <div className={twMerge("flex flex-col rounded-lg border border-cyan-900 backdrop-blur-[5px]", className)}>
      <table className="w-full table-fixed border-collapse">
        <TableHead columns={columns} />
        <TableBody
          columns={columns}
          items={items}
          noRowsOverlay={noRowsOverlay}
          breakpoint={breakpoint}
          itemRenderer={itemRenderer}
        />
        {footer && <TableFooter colspan={columns.length} footer={footer} />}
      </table>
    </div>
  )
})
