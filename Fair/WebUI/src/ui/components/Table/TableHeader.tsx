import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { TableProps } from "./types"

type TableHeaderProps = Pick<TableProps, "columns">

export const TableHeader = memo(({ columns }: TableHeaderProps) => (
  <div className="flex h-9 items-center justify-between gap-6 bg-gray-200 px-4 text-2xs font-medium leading-5">
    {columns.map(({ className, accessor, label, title }) => (
      <div className={twMerge(className ?? "flex-1", "select-none")} key={accessor} title={title}>
        {label}
      </div>
    ))}
  </div>
))
