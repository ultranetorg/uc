import { SkeletonBlock } from "ui/components"

const ITEMS_COUNT = 7

export const FavoriteStoresSkeleton = () => (
  <div className="flex items-center gap-2">
    {Array.from({ length: ITEMS_COUNT + 1 }, (_, i) => (
      <SkeletonBlock key={i} className="size-8 shrink-0" rounded="sm" tone="dark" />
    ))}
  </div>
)
