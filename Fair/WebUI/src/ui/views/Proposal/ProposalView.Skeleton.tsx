import { memo } from "react"

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
      <div className="flex gap-8">
        <div className="flex w-full flex-col gap-8">
          {!isPublication && <OptionsCollapsesListSkeleton className="max-w-187.5" />}
          <hr className="h-px border-0 bg-gray-300" />
          <div className="flex flex-col gap-4">
            <SkeletonBlock className="h-6 w-28" rounded="xs" />
            <CommentsListSkeleton count={2} hideRating={true} />
          </div>
        </div>
        <div className="flex flex-col gap-6">
          <ProposalInfoSkeleton className="w-87.5" />
          {!isPublication && <ButtonSkeleton className="w-full" />}
        </div>
      </div>
    </div>
  )
})
