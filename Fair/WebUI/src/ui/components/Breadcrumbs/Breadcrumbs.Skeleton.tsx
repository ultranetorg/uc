import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { SkeletonBlock } from "ui/components"

export type BreadcrumbsSkeletonProps = {
  items?: number
}

export const BreadcrumbsSkeleton = memo(({ items = 2 }: BreadcrumbsSkeletonProps) => (
  <div className="flex h-6 items-center gap-3">
    {Array.from({ length: items }, (_, i) => (
      <SkeletonBlock key={i} className={twMerge("h-3.5", i > 0 ? "w-24" : "w-12")} rounded="xs" />
    ))}
  </div>
))
