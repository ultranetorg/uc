import { SkeletonBlock } from "ui/components"
import { PublicationsGridSkeleton } from "ui/components/specific"

const SECTIONS_COUNT = 2
const SECTION_ITEMS_COUNT = 6

export const StorePageSkeleton = () => (
  <div className="flex flex-col gap-6">
    {Array.from({ length: SECTIONS_COUNT }, (_, i) => (
      <div key={i} className="flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <SkeletonBlock className="h-6 w-48" rounded="xs" />
          <SkeletonBlock className="h-4 w-16" rounded="xs" />
        </div>
        <PublicationsGridSkeleton count={SECTION_ITEMS_COUNT} />
      </div>
    ))}
  </div>
)
