import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { SkeletonBlock, SkeletonText } from "ui/components"

const DEFAULT_ROWS = 6
const YOUR_VOTE_FIELD_INDEX = 4

type PolicySurveyInfoSkeletonBaseProps = {
  rows?: number
}

export type PolicySurveyInfoSkeletonProps = PolicySurveyInfoSkeletonBaseProps & PropsWithClassName

export const PolicySurveyInfoSkeleton = memo(({ className, rows = DEFAULT_ROWS }: PolicySurveyInfoSkeletonProps) => (
  <div
    className={twMerge("flex max-h-fit flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6", className)}
  >
    {Array.from({ length: rows }, (_, i) => (
      <div key={i} className="flex flex-col gap-2">
        <SkeletonBlock className="my-0.5 h-4 w-20" rounded="xs" />
        <SkeletonBlock
          className={twMerge("my-0.5 h-4 w-40 max-w-full", YOUR_VOTE_FIELD_INDEX === i && "h-5")}
          rounded="xs"
        />
      </div>
    ))}
    <SkeletonText lines={3} textClassName="my-0.5 h-3" className="gap-0" />
  </div>
))
