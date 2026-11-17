import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { SkeletonBlock } from "ui/components"

import { getFavoriteStoreVisibilityClassName } from "./utils"

const ITEMS_COUNT = 7

export const FavoriteStoresSkeleton = memo(({ className }: PropsWithClassName) => (
  <div className={twMerge("flex items-center gap-2", className)}>
    {Array.from({ length: ITEMS_COUNT + 1 }, (_, i) => (
      <SkeletonBlock
        key={i}
        className={twMerge("size-8 shrink-0", i < ITEMS_COUNT && getFavoriteStoreVisibilityClassName(i))}
        rounded="sm"
        tone="dark"
      />
    ))}
  </div>
))
