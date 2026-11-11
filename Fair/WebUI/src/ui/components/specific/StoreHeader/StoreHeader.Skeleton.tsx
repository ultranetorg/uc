import { SkeletonBlock } from "ui/components"

export const StoreHeaderSkeleton = () => (
  <div className="flex items-center justify-between gap-8 pb-8">
    <div className="flex w-135 items-center justify-between gap-4">
      <div className="box-border flex h-11 w-full items-center justify-between gap-3 rounded-sm border border-gray-300 bg-gray-100 pr-3 pl-2.5">
        <SkeletonBlock className="h-4 w-24" rounded="xs" />
        <SkeletonBlock className="size-5 shrink-0" rounded="full" />
      </div>
    </div>
  </div>
)
