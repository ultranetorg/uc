import { TFunction } from "i18next"
import { PublicationDetails, Review, TotalItemsResult } from "types"

export type ContentProps = {
  t: TFunction
  siteId: string
  publication: PublicationDetails
  isPending: boolean
  isPendingReviews: boolean
  reviews?: TotalItemsResult<Review>
  error?: Error
  onLeaveReview: () => void
}
