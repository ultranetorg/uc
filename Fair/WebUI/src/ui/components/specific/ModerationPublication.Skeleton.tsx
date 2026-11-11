import { memo } from "react"

import { SkeletonBlock, SkeletonText } from "ui/components"

const FIELDS_LINES_COUNT = 6

export const ModerationPublicationSkeleton = memo(() => (
  <div className="flex flex-col gap-6 rounded-lg bg-gray-100 p-6">
    <div className="flex min-w-0 items-center gap-4">
      <SkeletonBlock className="size-17 shrink-0" rounded="2xl" />
      <div className="flex flex-1 flex-col gap-1">
        <SkeletonBlock className="my-1 h-6.5 w-72 max-w-full" rounded="xs" />
        <SkeletonBlock className="my-0.5 h-4 w-40" rounded="xs" />
      </div>
    </div>

    <div className="flex flex-col gap-4">
      <div className="flex gap-2">
        <SkeletonBlock className="my-px h-3.5 w-24" rounded="xs" />
        <SkeletonBlock className="my-px h-3.5 w-15" rounded="xs" />
      </div>
      <div className="flex gap-2">
        <SkeletonBlock className="my-px h-3.5 w-25" rounded="xs" />
        <SkeletonBlock className="my-px h-3.5 w-17" rounded="xs" />
      </div>
    </div>

    <SkeletonText lines={FIELDS_LINES_COUNT} />
  </div>
))
