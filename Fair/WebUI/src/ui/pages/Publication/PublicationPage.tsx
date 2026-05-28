import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { useModerationContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublicationDetails, useGetReviews } from "entities"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ReviewModal, PublicationHeader } from "ui/components/publication"
import { TEST_SOFTWARE_CATEGORIES } from "testConfig"
import { createBreadcrumbs } from "utils"
import { PublicationContentView } from "ui/views"

export const PublicationPage = () => {
  const { t } = useTranslation("publication")
  const { getOperationCreator: getOperationCreatorId } = useModerationContext()
  const { siteId, publicationId } = useParams()
  useDocumentTitle(t("title", { publicationId }))

  const create = getOperationCreatorId("review-creation")
  const edit = getOperationCreatorId("review-edit")

  const [isReviewModalOpen, setReviewModalOpen] = useState(false)
  const [editReview, setEditReview] = useState<{ id: string; text: string } | null>(null)

  const handleEditReview = useCallback((id: string, text: string) => setEditReview({ id, text }), [])

  const { isPending, data: publication } = useGetPublicationDetails(publicationId)
  const { isPending: isPendingReviews, data: reviews, error } = useGetReviews(publicationId, 0, DEFAULT_PAGE_SIZE_20)

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[] | undefined>(
    () =>
      publication
        ? createBreadcrumbs(
            siteId!,
            publication.categoryId!,
            publication.categoryTitle!,
            publication.title ?? publication.id,
            t,
          )
        : undefined,
    [publication, siteId, t],
  )

  if (isPending || !publication) {
    return <div>Loading</div>
  }

  return (
    <>
      <div className="flex flex-col gap-6">
        <Breadcrumbs items={breadcrumbsItems!} />
        <PublicationHeader
          id={publicationId!}
          title={publication.title}
          logoFileId={publication.logoId}
          categories={TEST_SOFTWARE_CATEGORIES}
        />
        <div className="flex gap-8">
          <PublicationContentView
            isPending={isPending}
            isPendingReviews={isPendingReviews}
            productOrPublication={publication}
            error={error}
            reviews={reviews}
            onLeaveReview={create ? () => setReviewModalOpen(true) : undefined}
            onEditReview={handleEditReview}
          />
        </div>
      </div>
      {isReviewModalOpen && (
        <ReviewModal
          publicationId={publication.id}
          title={t("leaveReview")}
          onClose={() => setReviewModalOpen(false)}
          onSubmit={() => setReviewModalOpen(false)}
          cancelLabel={t("common:cancel")}
          submitLabel={t("submitReview")}
          thankYouLabel={t("thankYou")}
          writeReviewLabel={t("writeReview")}
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
          onSubmit={() => setEditReview(null)}
          cancelLabel={t("common:cancel")}
          submitLabel={t("common:saveChanges")}
          thankYouLabel={t("thankYou")}
          writeReviewLabel={t("writeReview")}
          yourRatingLabel={t("yourRating")}
          yourReviewLabel={t("yourReview")}
        />
      )}
    </>
  )
}
