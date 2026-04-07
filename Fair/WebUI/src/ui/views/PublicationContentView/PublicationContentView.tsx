import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { ProductDetails, PublicationDetails } from "types"

import { ContentProps } from "./types"
import { getPublicationContentByType } from "./utils"

type PublicationContentViewBaseProps = {
  productOrPublication: ProductDetails | PublicationDetails
}

export type PublicationContentViewProps = PublicationContentViewBaseProps &
  Omit<ContentProps, "publication" | "siteId" | "t">

export const PublicationContentView = ({
  isPending,
  isPendingReviews,
  productOrPublication,
  error,
  reviews,
  onLeaveReview,
}: PublicationContentViewProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("publication")

  const ContentComponent = getPublicationContentByType(productOrPublication.type)

  return (
    <ContentComponent
      t={t}
      isPending={isPending}
      isPendingReviews={isPendingReviews}
      productOrPublication={productOrPublication}
      siteId={siteId!}
      error={error}
      reviews={reviews}
      onLeaveReview={onLeaveReview}
    />
  )
}
