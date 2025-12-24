import { memo } from "react"

import { TotalItemsResult, Review } from "types"
import { ButtonOutline } from "ui/components"
import { Comment } from "ui/components/comments/Comment"

import { ReviewsListEmptyState } from "./ReviewsListEmptyState"

export type ReviewsListProps = {
  isPending: boolean
  reviews?: TotalItemsResult<Review>
  error?: Error
  onLeaveReviewClick(): void
  leaveReviewLabel: string
  noReviewsLabel: string
  showMoreReviewsLabel: string
  reviewLabel: string
}

export const ReviewsList = memo(
  ({
    isPending,
    reviews,
    error,
    onLeaveReviewClick,
    leaveReviewLabel,
    noReviewsLabel,
    showMoreReviewsLabel,
    reviewLabel,
  }: ReviewsListProps) => (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <div className="flex gap-2 text-xl font-extrabold leading-6">
          <span className="text-gray-800">{reviewLabel}</span>
          <span className="text-gray-500">{reviews?.totalItems}</span>
        </div>
        <ButtonOutline label={leaveReviewLabel} onClick={onLeaveReviewClick} />
      </div>
      {error ? (
        <div>{error.message}</div>
      ) : isPending || !reviews ? (
        <div>Loading...</div>
      ) : reviews.items.length === 0 ? (
        <ReviewsListEmptyState label={noReviewsLabel} />
      ) : (
        <>
          {reviews.items.map(r => (
            <Comment key={r.id} text={r.text} rating={r.rating} account={r.creatorAccount} created={r.created} />
          ))}
          <ButtonOutline className="mx-auto" label={showMoreReviewsLabel} />
        </>
      )}
    </div>
  ),
)
