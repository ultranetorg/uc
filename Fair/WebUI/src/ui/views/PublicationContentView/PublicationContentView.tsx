import { useTranslation } from "react-i18next"

import { useResolveStoreId } from "hooks"
import { ProductDetails, PublicationDetails } from "types"

import { ContentProps } from "./types"
import { getPublicationContentByType } from "./utils"

type PublicationContentViewBaseProps = {
  productOrPublication: ProductDetails | PublicationDetails
}

export type PublicationContentViewProps = PublicationContentViewBaseProps &
  Omit<ContentProps, "publication" | "storeId" | "t">

export const PublicationContentView = ({
  isPending,
  isPendingReviews,
  productOrPublication,
  error,
  reviews,
  onLeaveReview,
  onEditReview,
}: PublicationContentViewProps) => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("publicationPage")

  const ContentComponent = getPublicationContentByType(productOrPublication.type)

  return (
    <ContentComponent
      t={t}
      isPending={isPending}
      isPendingReviews={isPendingReviews}
      productOrPublication={productOrPublication}
      storeId={storeId!}
      error={error}
      reviews={reviews}
      onLeaveReview={onLeaveReview}
      onEditReview={onEditReview}
    />
  )
}
