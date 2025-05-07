import { memo } from "react"

import { TotalItemsResult, Review } from "types"

import { Review as ReviewComponent } from "./Review"

export type ReviewsListProps = {
  isPending: boolean
  reviews?: TotalItemsResult<Review>
  error?: Error
}

export const ReviewsList = memo(({ isPending, reviews, error }: ReviewsListProps) => (
  <div className="flex flex-col gap-4">
    {error ? (
      <div>{error.message}</div>
    ) : isPending || !reviews ? (
      <div>⏱️ PENDING</div>
    ) : reviews.items.length === 0 ? (
      <div>🚫 NO REVIEWS</div>
    ) : (
      reviews.items.map(r => (
        <ReviewComponent
          key={r.id}
          text={r.text}
          rating={r.rating}
          userId={r.accountId}
          userName={r.accountNickname || r.accountAddress}
          created={r.created}
        />
      ))
    )}
  </div>
))
