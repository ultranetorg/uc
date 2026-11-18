import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { ButtonSkeleton, CommentsListSkeleton, SkeletonBlock } from "ui/components"
import { OptionsCollapsesListSkeleton, ProposalInfoSkeleton } from "ui/components/proposal"
import { ModerationHeaderSkeleton, ModerationPublicationSkeleton } from "ui/components/specific"

export type ProposalViewSkeletonVariant =
  | "default" // proposal with options list
  | "publication" // proposal for a publication operation

export type ProposalViewSkeletonProps = {
  variant?: ProposalViewSkeletonVariant
}

export const ProposalViewSkeleton = memo(({ variant = "default" }: ProposalViewSkeletonProps) => {
  const isPublication = variant === "publication"

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeaderSkeleton showSubtitle showActions={isPublication} />
      {isPublication && <ModerationPublicationSkeleton />}
      {/* Mirrors `ProposalView`: options, info card, comments when stacked; a two-column grid from `lg`. */}
      <div
        className={twMerge(
          "flex flex-col gap-8 lg:grid lg:grid-cols-[minmax(0,1fr)_21.875rem] lg:items-start",
          !isPublication && "lg:grid-rows-[auto_1fr]",
        )}
      >
        {!isPublication && (
          <div className="flex min-w-0 flex-col gap-8 lg:col-start-1">
            <OptionsCollapsesListSkeleton className="lg:max-w-187.5" />
          </div>
        )}
        <div
          className={twMerge(
            "flex w-full flex-col gap-6 lg:col-start-2 lg:row-start-1",
            !isPublication && "lg:row-span-2",
          )}
        >
          <ProposalInfoSkeleton className="w-full" />
          {!isPublication && <ButtonSkeleton className="w-full" />}
        </div>
        <div className="flex min-w-0 flex-col gap-8 lg:col-start-1">
          <hr className="h-px border-0 bg-gray-300" />
          <div className="flex flex-col gap-4">
            <SkeletonBlock className="h-6 w-28" rounded="xs" />
            <CommentsListSkeleton count={2} hideRating={true} />
          </div>
        </div>
      </div>
    </div>
  )
})
