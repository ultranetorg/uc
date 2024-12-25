import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { TableColumn } from "./types"

export type TableHeadProps = {
  columns: TableColumn[]
}

export const TableHead = memo((props: TableHeadProps) => {
  const { columns } = props

  return (
    <thead className="h-14">
      <tr className="border-b border-b-[#ffffff2a]">
        {columns.map(column => {
          return (
            <th
              className={twMerge("overflow-hidden text-ellipsis whitespace-nowrap px-4", column.className)}
              key={column.accessor}
            >
              {column.label}
            </th>
          )
        })}
      </tr>
    </thead>
  )
})
