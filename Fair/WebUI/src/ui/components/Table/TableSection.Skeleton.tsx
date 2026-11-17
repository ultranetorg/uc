import { memo } from "react"

import { SkeletonBlock } from "ui/components"

type TableSectionSkeletonBaseProps = {
  rows?: number
  withCategory?: boolean
}

export type TableSectionSkeletonProps = TableSectionSkeletonBaseProps

export const TableSectionSkeleton = memo(({ rows = 20, withCategory = false }: TableSectionSkeletonProps) => (
  <>
    <div className="flex flex-wrap items-center justify-between gap-3">
      <SkeletonBlock className="h-6 w-40" rounded="xs" />
      <SkeletonBlock className="h-10 w-48 max-w-full" rounded="sm" />
    </div>
    <div className="flex flex-col overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
      <div className="hidden h-8 bg-gray-200 sm:block" />
      <div className="divide-y divide-gray-300">
        {Array.from({ length: rows }, (_, i) => (
          <div
            key={i}
            className="flex flex-col items-start gap-3 p-4 sm:h-17 sm:flex-row sm:items-center sm:justify-between sm:p-0 sm:px-4"
          >
            <div className="flex w-full min-w-0 items-center gap-2 sm:w-[43%]">
              <SkeletonBlock className="size-8 shrink-0" rounded="lg" />
              <SkeletonBlock className="h-4 w-40 max-w-full" rounded="xs" />
            </div>
            {withCategory && (
              <div className="w-full sm:w-[30%]">
                <SkeletonBlock className="my-0.5 h-4 w-32 max-w-full" rounded="xs" />
              </div>
            )}
            <div className="flex w-full sm:w-[27%] sm:justify-center">
              <SkeletonBlock className="h-9 w-28 max-w-full" rounded="sm" />
            </div>
          </div>
        ))}
      </div>
    </div>
    <div className="flex justify-end">
      <SkeletonBlock className="h-10 w-48 max-w-full" rounded="sm" />
    </div>
  </>
))
