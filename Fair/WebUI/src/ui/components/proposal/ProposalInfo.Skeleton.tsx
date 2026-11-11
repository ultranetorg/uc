import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { SkeletonBlock } from "ui/components"

const DEFAULT_ROWS = 6
const YOUR_VOTE_FIELD_INDEX = 4

type ProposalInfoSkeletonBaseProps = {
  rows?: number
}

export type ProposalInfoSkeletonProps = ProposalInfoSkeletonBaseProps & PropsWithClassName

export const ProposalInfoSkeleton = memo(({ className, rows = DEFAULT_ROWS }: ProposalInfoSkeletonProps) => (
  <div
    className={twMerge("flex max-h-fit flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6", className)}
  >
    <div className="flex flex-col gap-2">
      <SkeletonBlock className="my-0.5 h-4 w-20" rounded="xs" />
      <div className="flex items-center gap-2">
        <SkeletonBlock className="size-8 shrink-0" rounded="full" />
        <SkeletonBlock className="h-4 w-40" rounded="xs" />
      </div>
    </div>
    {Array.from({ length: rows }, (_, i) => (
      <div className="flex flex-col gap-2" key={i}>
        <SkeletonBlock className="my-0.5 h-4 w-20" rounded="xs" />
        <SkeletonBlock
          className={twMerge("my-0.5 h-4 w-40 max-w-full", YOUR_VOTE_FIELD_INDEX === i && "h-5")}
          rounded="xs"
        />
      </div>
    ))}
  </div>
))
