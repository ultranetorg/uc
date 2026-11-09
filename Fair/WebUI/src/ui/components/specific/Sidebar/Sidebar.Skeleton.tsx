import { SkeletonBlock } from "ui/components"

const ROOT_ITEMS_COUNT = 8

export const SidebarSkeleton = () => (
  <div className="flex flex-col gap-8">
    <div className="flex h-11 items-center gap-3">
      <SkeletonBlock className="size-8 shrink-0" rounded="lg" />
      <div className="flex min-w-0 flex-1 flex-col gap-1.5">
        <SkeletonBlock className="h-3.5 w-32" rounded="xs" />
        <SkeletonBlock className="h-3 w-20" rounded="xs" />
      </div>
    </div>
    <div className="flex flex-col">
      {Array.from({ length: ROOT_ITEMS_COUNT }, (_, i) => (
        <div key={i} className="mb-1 flex h-9 items-center gap-1 p-1.5">
          <span className="size-6 shrink-0" />
          <SkeletonBlock className="size-5 shrink-0" rounded="md" />
          <SkeletonBlock className="h-3.5 w-32" rounded="xs" />
        </div>
      ))}
    </div>
  </div>
)
