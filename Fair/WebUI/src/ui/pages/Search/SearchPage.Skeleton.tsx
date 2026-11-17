import { memo } from "react"

import { SkeletonBlock } from "ui/components"
import { PublicationsListSkeleton } from "ui/components/specific"

const ITEMS_COUNT = 20

type SearchPageSkeletonProps = {
  showPublicationType?: boolean
}

export const SearchPageSkeleton = memo(({ showPublicationType }: SearchPageSkeletonProps) => (
  <div className="flex flex-col gap-6">
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between sm:gap-4">
      <SkeletonBlock className="h-5 w-32" rounded="xs" />
      <div className="grid grid-cols-2 items-center gap-3 sm:grid-cols-[repeat(2,9.375rem)] sm:gap-4">
        <SkeletonBlock className="h-10 w-full" rounded="sm" />
        <SkeletonBlock className="h-10 w-full" rounded="sm" />
      </div>
    </div>
    <PublicationsListSkeleton rows={ITEMS_COUNT} showPublicationType={showPublicationType} showRating />
  </div>
))
