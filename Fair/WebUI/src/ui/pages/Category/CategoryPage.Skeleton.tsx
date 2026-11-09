import { SkeletonBlock } from "ui/components"
import { PublicationsGridSkeleton, PublicationsListSkeleton, ViewType } from "ui/components/specific"

const GRID_ITEMS_COUNT = 12
const LIST_ROWS_COUNT = 20

type CategoryPageSkeletonProps = {
  view?: ViewType
}

export const CategoryPageSkeleton = ({ view = "grid" }: CategoryPageSkeletonProps) => (
  <div className="flex flex-col gap-6">
    <div className="flex items-center justify-between">
      <SkeletonBlock className="h-10 w-90" rounded="sm" />
      <SkeletonBlock className="h-10.5 w-20" rounded="sm" />
    </div>
    {view === "list" ? (
      <PublicationsListSkeleton rows={LIST_ROWS_COUNT} />
    ) : (
      <PublicationsGridSkeleton count={GRID_ITEMS_COUNT} />
    )}
  </div>
)
