import { SkeletonBlock } from "ui/components"

export const NextPaginationSkeleton = () => (
  <div className="flex gap-4">
    <SkeletonBlock className="size-9 shrink-0" rounded="sm" />
    <SkeletonBlock className="size-9 shrink-0" rounded="sm" />
    <SkeletonBlock className="size-9 shrink-0" rounded="sm" />
    <SkeletonBlock className="size-9 shrink-0" rounded="sm" />
    <SkeletonBlock className="h-9 w-16.25 shrink-0" rounded="sm" />
  </div>
)
