import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { SkeletonBlock } from "./SkeletonBlock"

type SkeletonTextBaseProps = {
  lines?: number
}

export type SkeletonTextProps = PropsWithClassName & SkeletonTextBaseProps

export const SkeletonText = memo(({ className, lines = 3 }: SkeletonTextProps) => (
  <div className={twMerge("flex flex-col gap-2", className)}>
    {Array.from({ length: lines }, (_, i) => (
      <SkeletonBlock key={i} className={twMerge("h-4 w-full", lines > 1 && i === lines - 1 && "w-3/5")} rounded="xs" />
    ))}
  </div>
))
