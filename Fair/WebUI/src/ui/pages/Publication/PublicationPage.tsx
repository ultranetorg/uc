import { useMemo, useState } from "react"
import { Outlet, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { useGetPublication, useGetReviews } from "entities"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ReviewModal, SoftwarePublicationHeader } from "ui/components/publication"
import { TEST_SOFTWARE_CATEGORIES } from "testConfig"
import { createBreadcrumbs } from "utils"
import { ReviewsList } from "ui/components/specific"
import { DEFAULT_PAGE_SIZE_20 } from "config"

export const PublicationPage = () => {
  const { t } = useTranslation("publication")
  const { siteId, publicationId } = useParams()
  useDocumentTitle(t("title", { publicationId }))

  const [isReviewModalOpen, setReviewModalOpen] = useState(false)

  const { isPending, data: publication } = useGetPublication(publicationId)
  const { isPending: isPendingReviews, data: reviews, error } = useGetReviews(publicationId, 0, DEFAULT_PAGE_SIZE_20)

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[] | undefined>(
    () =>
      publication
        ? createBreadcrumbs(siteId!, publication.categoryId, publication.categoryTitle, publication.title, t)
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
        <SoftwarePublicationHeader
          id={publicationId!}
          title={publication.title}
          logoFileId={publication.logoFileId}
          categories={TEST_SOFTWARE_CATEGORIES}
        />
        <div className="flex flex-1 flex-col gap-8">
          <Outlet />

          <ReviewsList
            isPending={isPending || isPendingReviews}
            reviews={reviews}
            error={error}
            onLeaveReviewClick={() => setReviewModalOpen(true)}
            leaveReviewLabel={t("leaveReview")}
            noReviewsLabel={t("noReviews")}
            reviewLabel={t("review", { count: reviews?.totalItems })}
            showMoreReviewsLabel={t("showMoreReviews")}
          />
        </div>
      </div>
      {isReviewModalOpen && (
        <ReviewModal
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
    </>
  )
}
