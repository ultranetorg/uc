import { SkeletonBlock } from "ui/components"

export const SearchInputSkeleton = () => (
  <div className="box-border flex h-auto w-full items-center rounded-sm border border-gray-300 bg-gray-100 px-3 py-2.25">
    <div className="flex h-6 w-full items-center gap-3">
      <SkeletonBlock className="h-4 w-20 shrink-0" rounded="xs" />
      <div className="h-5 w-px shrink-0 bg-gray-300" />
      <SkeletonBlock className="h-4 flex-1" rounded="xs" />
      <SkeletonBlock className="size-5 shrink-0" rounded="full" />
    </div>
  </div>
)
