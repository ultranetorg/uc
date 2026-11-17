import { memo } from "react"

import { SkeletonBlock, SkeletonText } from "ui/components"

type CommentsListSkeletonProps = {
  count?: number
  hideRating?: boolean
}

export const CommentsListSkeleton = memo(({ count = 3, hideRating }: CommentsListSkeletonProps) => (
  <div className="flex flex-col gap-4">
    {Array.from({ length: count }, (_, i) => (
      <div key={i} className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-4 sm:p-6">
        <div className="flex flex-col gap-4">
          <div className="flex gap-3 sm:gap-4.5">
            <SkeletonBlock className="size-13 shrink-0" rounded="full" />
            <div className="flex min-w-0 flex-1 flex-col justify-center gap-2">
              <SkeletonBlock className="h-4.5 w-40 max-w-full" rounded="xs" />
              <SkeletonBlock className="h-3.5 w-24 max-w-full" rounded="xs" />
            </div>
          </div>
          {hideRating !== true && <SkeletonBlock className="h-4.5 w-28" rounded="xs" />}
        </div>
        <SkeletonText textClassName="h-5" lines={2} />
      </div>
    ))}
  </div>
))
