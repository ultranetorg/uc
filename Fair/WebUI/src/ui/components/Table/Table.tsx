import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useBreakpoint } from "hooks"

import { TableHead, TableHeadProps } from "./TableHead"
import { TableBody, TableBodyProps } from "./TableBody"
import { TableFooter, TableFooterProps } from "./TableFooter"

type TableBaseProps = {
  className?: string
  title?: string | null
}

type TableProps = TableBaseProps &
  TableHeadProps &
  Omit<TableBodyProps, "breakpoint"> &
  Pick<TableFooterProps, "footer">

export const Table = memo((props: TableProps) => {
  const { className, title, columns, items, itemRenderer, noRowsOverlay, footer } = props

  const breakpoint = useBreakpoint()

  return (
    <div
      className={twMerge(
        "flex flex-col rounded-md border border-solid border-[#3dc1f2] bg-[#1a1a1d] px-4 pb-2 pt-0 xs:px-8 xs:pt-1",
        className,
      )}
    >
      {title && <div className="mb-2.5 mt-4 w-full overflow-hidden text-ellipsis whitespace-nowrap">{title}</div>}
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
