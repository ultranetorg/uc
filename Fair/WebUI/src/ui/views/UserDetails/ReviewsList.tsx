import { memo } from "react"

import { Review, TotalItemsResult } from "types"
import { Comment } from "ui/components/comments"

export type ReviewsListProps = {
  reviews: TotalItemsResult<Review>
}

export const ReviewsList = memo(({ reviews }: ReviewsListProps) => (
  <div className="grid grid-cols-1 gap-3">
    {reviews.items.map(x => (
      <Comment
        key={x.id}
        style="compact"
        id={x.id}
        text={x.text}
        rating={x.rating}
        user={x.creatorUser}
        created={x.created}
        publication={{ id: x.publicationId, title: x.publicationTitle }}
      />
    ))}
  </div>
))
