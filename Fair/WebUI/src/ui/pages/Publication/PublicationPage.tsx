import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { useGetPublication, useGetReviews } from "entities"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ReviewModal, SoftwarePublicationHeader } from "ui/components/publication"
import { TEST_SOFTWARE_CATEGORIES } from "testConfig"
import { createBreadcrumbs } from "utils"

import { getPublicationContentByType } from "./utils"

export const PublicationPage = () => {
  const { t } = useTranslation("publication")
  const { siteId, publicationId } = useParams()
  useDocumentTitle(publicationId ? `Publication - ${publicationId} | Ultranet Fair` : "Publication | Ultranet Fair")

  const [isReviewModalOpen, setReviewModalOpen] = useState(false)
  const [reviewsPage] = useState(0)
  const [reviewPageSize] = useState(20)

  const { isPending, data: publication } = useGetPublication(publicationId)
  const {
    isPending: isPendingReviews,
    data: reviews,
    error,
  } = useGetReviews(publicationId, reviewsPage, reviewPageSize)

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[] | undefined>(
    () =>
      publication
        ? createBreadcrumbs(siteId!, publication.categoryId, publication.categoryTitle, publication.title, t)
        : undefined,
    [publication, siteId, t],
  )

  const handleLeaveReview = useCallback(() => setReviewModalOpen(true), [])
  const handleReviewModalClose = useCallback(() => setReviewModalOpen(false), [])
  const handleReviewModalSubmit = useCallback(() => setReviewModalOpen(false), [])

  if (isPending || !publication) {
    return <div>Loading</div>
  }

  const ContentComponent = getPublicationContentByType(publication.productType)

  return (
    <>
      <div className="flex flex-col gap-6">
        <Breadcrumbs items={breadcrumbsItems!} />
        <SoftwarePublicationHeader
          id={publicationId!}
          title={publication.title}
          logoFileId={publication.logoFileId}
          categories={TEST_SOFTWARE_CATEGORIES}
        />
        <div className="flex gap-8">
          <ContentComponent
            t={t}
            isPending={isPending}
            isPendingReviews={isPendingReviews}
            publication={publication}
            siteId={siteId!}
            error={error}
            reviews={reviews}
            onLeaveReview={handleLeaveReview}
          />
        </div>
      </div>
      {isReviewModalOpen && (
        <ReviewModal
          title={t("leaveReview")}
          onClose={handleReviewModalClose}
          onSubmit={handleReviewModalSubmit}
          cancelLabel={t("common:cancel")}
          submitLabel={t("submitReview")}
          thankYouLabel={t("thankYou")}
          writeReviewLabel={t("writeReview")}
          yourRatingLabel={t("yourRating")}
          yourReviewLabel={t("yourReview")}
        />
      )}
    </>
  )
}
