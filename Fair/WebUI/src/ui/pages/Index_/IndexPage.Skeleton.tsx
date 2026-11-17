import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { ListSkeleton, NextPaginationSkeleton, SkeletonBlock } from "ui/components"
import { SearchInputSkeleton, SearchScope } from "ui/components/specific"

const FILTER_WIDTHS = ["w-17.5", "w-29", "w-25.25", "w-23.25", "w-23.75", "w-24"]

const NAME_CLASSNAME = "flex-1 sm:flex-2"
const PRODUCTS_COLUMNS = ["hidden w-20 shrink-0 md:block", "hidden flex-1 sm:block", "flex-1"]
const STORES_COLUMNS: string[] = []

type IndexPageSkeletonProps = {
  scope?: SearchScope
}

export const IndexPageSkeleton = memo(({ scope = "products" }: IndexPageSkeletonProps) => (
  <div className="flex flex-col items-center gap-8 px-4 py-6 sm:gap-12 sm:px-6 sm:py-8">
    <div className="flex w-full max-w-225 flex-col items-center gap-6">
      <div className="flex w-full flex-col items-center gap-4">
        <SearchInputSkeleton />
        {scope === "products" && (
          <div className="flex flex-wrap justify-center gap-2 sm:gap-3">
            {FILTER_WIDTHS.map(width => (
              <SkeletonBlock key={width} className={twMerge("h-9 shrink-0", width)} rounded="sm" />
            ))}
          </div>
        )}
      </div>
      <div className="flex w-full flex-col gap-4">
        <SkeletonBlock className="h-5 w-32" rounded="xs" />
        <ListSkeleton
          className="rounded-sm"
          compact
          showHeader
          nameClassName={NAME_CLASSNAME}
          columns={scope === "products" ? PRODUCTS_COLUMNS : STORES_COLUMNS}
        />
        {scope === "products" && <NextPaginationSkeleton />}
      </div>
    </div>
  </div>
))
