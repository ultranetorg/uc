import { memo } from "react"

import { SkeletonBlock } from "ui/components"
import { PublicationsListSkeleton } from "ui/components/specific"

const ITEMS_COUNT = 20

type SearchPageSkeletonProps = {
  showPublicationType?: boolean
}

export const SearchPageSkeleton = memo(({ showPublicationType }: SearchPageSkeletonProps) => (
  <div className="flex flex-col gap-6">
    <div className="flex items-center justify-between">
      <SkeletonBlock className="h-5 w-32" rounded="xs" />
      <div className="flex items-center gap-4">
        <SkeletonBlock className="h-10 w-37.5" rounded="sm" />
        <SkeletonBlock className="h-10 w-37.5" rounded="sm" />
      </div>
    </div>
    <PublicationsListSkeleton rows={ITEMS_COUNT} showPublicationType={showPublicationType} showRating />
  </div>
))
