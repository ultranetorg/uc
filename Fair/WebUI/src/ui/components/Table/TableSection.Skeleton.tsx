import { memo } from "react"

import { SkeletonBlock } from "ui/components"

type TableSectionSkeletonBaseProps = {
  rows?: number
  withCategory?: boolean
}

export type TableSectionSkeletonProps = TableSectionSkeletonBaseProps

export const TableSectionSkeleton = memo(({ rows = 20, withCategory = false }: TableSectionSkeletonProps) => (
  <>
    <div className="flex items-center justify-between">
      <SkeletonBlock className="h-6 w-40" rounded="xs" />
      <SkeletonBlock className="h-10 w-48" rounded="sm" />
    </div>
    <div className="flex flex-col overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
      <div className="h-8 bg-gray-200" />
      <div className="divide-y divide-gray-300">
        {Array.from({ length: rows }, (_, i) => (
          <div key={i} className="flex h-17 items-center justify-between px-4">
            <div className="flex w-[43%] items-center gap-2">
              <SkeletonBlock className="size-8 shrink-0" rounded="lg" />
              <SkeletonBlock className="h-4 w-40 max-w-full" rounded="xs" />
            </div>
            {withCategory && (
              <div className="w-[30%]">
                <SkeletonBlock className="h-4 w-32 max-w-full" rounded="xs" />
              </div>
            )}
            <div className="flex w-[27%] justify-center">
              <SkeletonBlock className="h-9 w-28 max-w-full" rounded="sm" />
            </div>
          </div>
        ))}
      </div>
    </div>
    <div className="flex justify-end">
      <SkeletonBlock className="h-10 w-48" rounded="sm" />
    </div>
  </>
))
