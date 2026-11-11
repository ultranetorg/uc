import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { ButtonSkeleton, SkeletonBlock } from "ui/components"

import { TableHeader } from "./TableHeader"
import { TableRow } from "./TableRow"
import { TableColumn } from "./types"

const DEFAULT_ROWS = 10

const getAlignmentClassName = (className?: string) => {
  if (className?.includes("text-right")) return "ml-auto"
  if (className?.includes("text-center")) return "mx-auto"
  return undefined
}

const getButtonsCount = ({ type }: TableColumn) => {
  if (type?.endsWith("action-buttons")) return 2
  if (type?.endsWith("action-button")) return 1
  return 0
}

type TableSkeletonBaseProps = {
  columns: TableColumn[]
  rows?: number
}

export type TableSkeletonProps = TableSkeletonBaseProps

export const TableSkeleton = memo(({ columns, rows = DEFAULT_ROWS }: TableSkeletonProps) => (
  <div className="overflow-hidden rounded-lg border border-gray-300">
    <table className="w-full table-fixed border-collapse bg-gray-100">
      <TableHeader columns={columns} />
      <tbody>
        {Array.from({ length: rows }, (_, i) => (
          <TableRow key={i}>
            {columns.map(column => {
              const buttonsCount = getButtonsCount(column)

              return (
                <td key={column.accessor} className={twMerge("px-4 py-3", column.className)}>
                  {buttonsCount > 0 ? (
                    <div className="flex justify-center gap-5">
                      {Array.from({ length: buttonsCount }, (_, j) => (
                        <ButtonSkeleton key={j} className="h-9 w-20" />
                      ))}
                    </div>
                  ) : (
                    <SkeletonBlock
                      className={twMerge("h-4 w-3/5", getAlignmentClassName(column.className))}
                      rounded="xs"
                    />
                  )}
                </td>
              )
            })}
          </TableRow>
        ))}
      </tbody>
    </table>
  </div>
))
