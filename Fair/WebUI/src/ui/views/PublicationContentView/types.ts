import { TFunction } from "i18next"
import { ProductDetails, PublicationDetails, Review, TotalItemsResult } from "types"

export type ContentProps = {
  t: TFunction
  siteId: string
  productOrPublication: ProductDetails | PublicationDetails
  isPending: boolean
  isPendingReviews?: boolean
  reviews?: TotalItemsResult<Review>
  error?: Error
  onLeaveReview?: () => void
}
