import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { TableColumn } from "./types"

export type TableHeadProps = {
  columns: TableColumn[]
}

export const TableHead = memo((props: TableHeadProps) => {
  const { columns } = props

  return (
    <thead className="h-10">
      <tr className="border-b-dark-alpha-75 bg-dark-alpha-75 border-b">
        {columns.map(column => (
          <th
            className={twMerge(
              "overflow-hidden text-ellipsis whitespace-nowrap px-4 text-[13px] font-medium text-gray-500",
              column.className,
            )}
            key={column.accessor}
          >
            {column.label}
          </th>
        ))}
      </tr>
    </thead>
  )
})
