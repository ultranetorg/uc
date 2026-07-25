import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { useOperationPolicy, useSignInContext, useStoreContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublicationDetails, useGetReviews } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ReviewModal, PublicationHeader } from "ui/components/publication"
import { createBreadcrumbs } from "utils"
import { PublicationContentView } from "ui/views"

export const PublicationPage = () => {
  const { t } = useTranslation("publicationPage")
  const { creator: create } = useOperationPolicy("review-creation")
  const { publicationId } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()

  const { startSignIn } = useSignInContext()

  const [isReviewModalOpen, setReviewModalOpen] = useState(false)
  const [editReview, setEditReview] = useState<{ id: string; text: string } | null>(null)

  const { isPending, data: publication, error } = useGetPublicationDetails(publicationId)
  if (error) throw error

  useStoreTitle(store?.title, publication?.title ? `Publication - ${publication?.title}` : undefined)

  const {
    isPending: isPendingReviews,
    data: reviews,
    error: reviewsError,
    refetch: refetchReviews,
  } = useGetReviews(publicationId, 0, DEFAULT_PAGE_SIZE_20)

  const handleEditReview = useCallback((id: string, text: string) => setEditReview({ id, text }), [])

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[] | undefined>(
    () =>
      publication ? createBreadcrumbs(storeId!, publication.path, publication.title ?? publication.id, t) : undefined,
    [publication, storeId, t],
  )

  const handleLeaveReview = useCallback(() => {
    if (create) setReviewModalOpen(true)
    else startSignIn("user")
  }, [create, startSignIn])

  const handleReviewSubmit = useCallback(() => {
    setReviewModalOpen(false)
    setEditReview(null)
    refetchReviews()
  }, [refetchReviews])

  if (isPending || !publication) {
    return <div>Loading</div>
  }

  return (
    <>
      <div className="flex flex-col gap-6">
        <Breadcrumbs items={breadcrumbsItems!} />
        <PublicationHeader id={publicationId!} title={publication.title} logoFileId={publication.logoId} />
        <div className="flex gap-8">
          <PublicationContentView
            isPending={isPending}
            isPendingReviews={isPendingReviews}
            productOrPublication={publication}
            error={reviewsError}
            reviews={reviews}
            onLeaveReview={handleLeaveReview}
            onEditReview={handleEditReview}
          />
        </div>
      </div>
      {isReviewModalOpen && (
        <ReviewModal
          publicationId={publication.id}
          title={t("writeReview")}
          onClose={() => setReviewModalOpen(false)}
          onSubmit={handleReviewSubmit}
          cancelLabel={t("common:cancel")}
          submitLabel={t("submitReview")}
          thankYouLabel={t("thankYou")}
          writeReviewLabel={t("writeYourReview")}
          yourRatingLabel={t("yourRating")}
          yourReviewLabel={t("yourReview")}
        />
      )}
      {editReview && (
        <ReviewModal
          publicationId={publication.id}
          reviewId={editReview.id}
          initialText={editReview.text}
          title={t("editReview")}
          onClose={() => setEditReview(null)}
          onSubmit={handleReviewSubmit}
          cancelLabel={t("common:cancel")}
          submitLabel={t("common:saveChanges")}
          thankYouLabel={t("thankYou")}
          writeReviewLabel={t("writeYourReview")}
          yourRatingLabel={t("yourRating")}
          yourReviewLabel={t("yourReview")}
        />
      )}
    </>
  )
}
