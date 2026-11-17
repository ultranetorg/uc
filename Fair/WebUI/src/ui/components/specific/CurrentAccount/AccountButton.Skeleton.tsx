import { SkeletonBlock } from "ui/components"

export const AccountButtonSkeleton = () => (
  <div className="flex h-full w-30 items-center gap-2 rounded-md bg-gray-600 p-1 sm:w-53.5">
    <SkeletonBlock className="size-10 shrink-0" rounded="full" tone="dark" />
    <SkeletonBlock className="h-4 w-14 sm:w-24" rounded="xs" tone="dark" />
  </div>
)
