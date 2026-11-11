import { memo } from "react"

import { SkeletonBlock } from "ui/components"

export type PublicationsGridSkeletonProps = {
  count?: number
}

export const PublicationsGridSkeleton = memo(({ count = 8 }: PublicationsGridSkeletonProps) => (
  <div className="flex flex-wrap gap-4">
    {Array.from({ length: count }, (_, i) => (
      <div key={i} className="flex h-40 w-67.75 flex-col items-center justify-center gap-4 rounded-lg bg-gray-100 p-4">
        <SkeletonBlock className="size-14 shrink-0" rounded="lg" />
        <div className="flex w-40 flex-col items-center gap-1">
          <SkeletonBlock className="h-4.5 w-5/6" rounded="xs" />
          <SkeletonBlock className="h-3.5 w-3/4" rounded="xs" />
          <SkeletonBlock className="h-3.5 w-full" rounded="xs" />
        </div>
      </div>
    ))}
  </div>
))
