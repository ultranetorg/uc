import { memo } from "react"

import { TableProps } from "./types"

type TableHeaderProps = Pick<TableProps, "columns">

export const TableHeader = memo(({ columns }: TableHeaderProps) => (
  <div className="flex h-9 items-center justify-between gap-6 bg-gray-200 px-4 text-2xs font-medium leading-5">
    {columns.map(x => (
      <div className={x.className ?? "flex-1"} key={x.accessor}>
        {x.label ?? "x"}
      </div>
    ))}
  </div>
))
