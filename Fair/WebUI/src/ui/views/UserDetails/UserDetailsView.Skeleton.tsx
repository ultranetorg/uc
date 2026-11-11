import { memo } from "react"

import { SkeletonBlock } from "ui/components"

const REVIEWS_COUNT = 5

export const UserDetailsViewSkeleton = memo(() => (
  <div className="divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
    <div className="flex flex-col">
      <div className="relative h-57.5">
        <SkeletonBlock className="h-42.5 w-full" rounded="none" />
        <div className="absolute bottom-0 left-6 size-32 rounded-full bg-white">
          <SkeletonBlock className="absolute top-1 left-1 size-30" rounded="full" />
        </div>
      </div>
      <div className="flex flex-col gap-4 p-6">
        <div className="flex flex-col">
          <SkeletonBlock className="h-6 w-48" rounded="xs" />
          <SkeletonBlock className="h-5 w-28" rounded="xs" />
        </div>
        <SkeletonBlock className="h-4 w-32" rounded="xs" />
      </div>
    </div>

    <div className="flex flex-col gap-5 p-6">
      <SkeletonBlock className="h-5 w-24" rounded="xs" />
      <div className="flex items-center gap-2">
        <SkeletonBlock className="size-8 shrink-0" rounded="full" />
        <SkeletonBlock className="h-5 w-32" rounded="xs" />
      </div>
    </div>

    <div className="flex flex-col gap-5 p-6">
      <SkeletonBlock className="h-5 w-32" rounded="xs" />
      <div className="flex flex-col gap-3">
        {Array.from({ length: REVIEWS_COUNT }, (_, i) => (
          <div key={i} className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-white p-4">
            <div className="flex items-center gap-2">
              <SkeletonBlock className="size-10 shrink-0" rounded="full" />
              <div className="flex flex-col gap-1">
                <SkeletonBlock className="h-4.5 w-40" rounded="xs" />
                <SkeletonBlock className="h-4 w-24" rounded="xs" />
              </div>
            </div>
            <SkeletonBlock className="h-4.5 w-32" rounded="xs" />
            <SkeletonBlock className="h-5 w-full" rounded="xs" />
          </div>
        ))}
      </div>
    </div>
  </div>
))
