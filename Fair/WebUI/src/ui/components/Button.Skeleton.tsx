import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { SkeletonBlock } from "./SkeletonBlock"

export type ButtonSkeletonProps = PropsWithClassName

export const ButtonSkeleton = memo(({ className }: ButtonSkeletonProps) => (
  <SkeletonBlock className={twMerge("h-11 w-40 shrink-0", className)} rounded="sm" />
))
