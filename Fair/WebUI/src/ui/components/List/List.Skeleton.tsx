import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { SkeletonBlock } from "ui/components"

const DEFAULT_COLUMNS = ["flex-1", "flex-1", "w-16 shrink-0"]

type ListSkeletonBaseProps = {
  rows?: number
  compact?: boolean
  showHeader?: boolean
  nameClassName?: string
  columns?: string[]
}

export type ListSkeletonProps = PropsWithClassName & ListSkeletonBaseProps

export const ListSkeleton = memo(
  ({
    className,
    rows = 10,
    compact = false,
    showHeader = false,
    nameClassName,
    columns = DEFAULT_COLUMNS,
  }: ListSkeletonProps) => (
    <div className={twMerge("overflow-hidden rounded-lg border border-gray-300 bg-gray-100", className)}>
      {showHeader && (
        <div
          className={twMerge("flex h-7.5 items-center gap-4 border-b border-gray-300 px-4", compact && "gap-6 px-2")}
        >
          <div className={twMerge("flex min-w-0 flex-2 items-center", nameClassName)}>
            <SkeletonBlock className="h-3 w-16" rounded="xs" />
          </div>
          {columns.map((column, j) => (
            <div key={j} className={twMerge("min-w-0", column)}>
              <SkeletonBlock className="h-3 w-12" rounded="xs" />
            </div>
          ))}
        </div>
      )}
      <div className="divide-y divide-gray-300">
        {Array.from({ length: rows }, (_, i) => (
          <div key={i} className={twMerge("flex items-center gap-4 p-4", compact && "gap-6 p-2")}>
            <div className={twMerge("flex min-w-0 flex-2 items-center gap-2", nameClassName)}>
              <SkeletonBlock className="size-8 shrink-0" rounded="lg" />
              <SkeletonBlock className="h-4 w-40 max-w-full" rounded="xs" />
            </div>
            {columns.map((column, j) => (
              <SkeletonBlock key={j} className={twMerge("h-4", column)} rounded="xs" />
            ))}
          </div>
        ))}
      </div>
    </div>
  ),
)
