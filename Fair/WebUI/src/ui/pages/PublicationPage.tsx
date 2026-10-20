import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { useOperationPolicy, useSignInContext, useStoreContext } from "app"
import { REVIEWS_PAGE_SIZE } from "config"
import { useGetPaginatedReviews, useGetPublicationDetails, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useParams, useStoreTitle } from "hooks"
import { BaseVotableOperation, ProposalCreation, ProposalOption } from "types"
import { TextModal } from "ui/components"
import { ReviewModal, PublicationHeader } from "ui/components/publication"
import { PublicationContentView } from "ui/views"
import { showToast } from "utils"

export const PublicationPage = () => {
  const { t } = useTranslation("publicationPage")
  const { creator: create } = useOperationPolicy("review-creation")
  const { creator: deleteCreator } = useOperationPolicy("review-deletion")
  const { publicationId } = useParams()
  const { store } = useStoreContext()

  const { startSignIn } = useSignInContext()
  const { mutate } = useTransactMutationWithStatus()
  const { invalidateOperation } = useInvalidation()

  const [isReviewModalOpen, setReviewModalOpen] = useState(false)
  const [editReview, setEditReview] = useState<{ id: string; text: string } | null>(null)
  const [deleteReview, setDeleteReview] = useState<{ id: string; text: string } | null>(null)

  const { isPending, data: publication, error } = useGetPublicationDetails(publicationId)
  if (error) throw error

  useStoreTitle(store?.title, publication?.title ? `Publication - ${publication?.title}` : undefined)

  const {
    reviews,
    isPending: isPendingReviews,
    error: reviewsError,
    hasMoreReviews,
    fetchNextReviews,
    refetch: refetchReviews,
  } = useGetPaginatedReviews(publicationId, REVIEWS_PAGE_SIZE)

  const handleEditReview = useCallback((id: string, text: string) => setEditReview({ id, text }), [])

  const handleDeleteReview = useCallback((id: string, text: string) => setDeleteReview({ id, text }), [])

  const handleDeleteConfirm = useCallback(() => {
    if (!deleteReview || !deleteCreator || !store) return

    const options = [
      {
        title: "",
        operation: { $type: "ReviewDeletion", Review: deleteReview.id } as unknown as BaseVotableOperation,
      },
    ] as ProposalOption[]

    const operation = new ProposalCreation(store.id, deleteCreator.id, deleteCreator.role, "", options, "")
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:reviewDeletionCreated"), "success")

        invalidateOperation("review-deletion", { storeId: store.id, publicationId })
        setDeleteReview(null)
        refetchReviews()
      },
      onError: err => showToast(err.toString(), "error"),
    })
  }, [deleteCreator, deleteReview, invalidateOperation, mutate, publicationId, refetchReviews, store, t])

  const handleLeaveReview = useCallback(() => {
    if (create) setReviewModalOpen(true)
    else startSignIn("user")
  }, [create, startSignIn])

  const handleReviewSubmit = useCallback(() => {
    setReviewModalOpen(false)
    setEditReview(null)
    refetchReviews()
  }, [refetchReviews])

  const handleShowMore = useCallback(() => fetchNextReviews(), [fetchNextReviews])

  if (isPending || !publication) {
    return <div>Loading{import.meta.env.DEV ? " (PublicationPage)" : ""}</div>
  }

  return (
    <>
      <div className="flex flex-col gap-6">
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
            onDeleteReview={handleDeleteReview}
            onShowMore={handleShowMore}
            hasMoreReviews={hasMoreReviews}
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
      {deleteReview && (
        <TextModal
          title={t("deleteReview")}
          text={t("deleteReviewConfirm")}
          cancelLabel={t("common:cancel")}
          confirmLabel={t("common:delete")}
          onCancel={() => setDeleteReview(null)}
          onConfirm={handleDeleteConfirm}
        />
      )}
    </>
  )
}
