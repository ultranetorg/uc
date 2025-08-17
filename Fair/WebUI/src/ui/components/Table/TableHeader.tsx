import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { TableProps } from "./types"

type TableHeaderProps = Pick<TableProps, "columns">

export const TableHeader = memo(({ columns }: TableHeaderProps) => (
  <thead className="h-8">
    <tr className="bg-gray-200">
      {columns.map(({ className, accessor, label, title }) => (
        <th
          className={twMerge(
            "select-none overflow-hidden text-ellipsis whitespace-nowrap px-4 text-left text-2xs font-medium leading-5",
            className,
          )}
          key={accessor}
          title={title || label}
        >
          {label}
        </th>
      ))}
    </tr>
  </thead>
))
