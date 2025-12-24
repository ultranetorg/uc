import { useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublication, useGetReviews } from "entities"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ReviewModal, SoftwarePublicationHeader } from "ui/components/publication"
import { createBreadcrumbs } from "utils"

import { getPublicationContentByType } from "./utils"

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

  const headerCategories = useMemo(() => {
    const pt = publication?.productType
    const ptLabel = pt ? pt.charAt(0).toUpperCase() + pt.slice(1) : ""
    return [ptLabel, publication?.categoryTitle].filter(Boolean) as string[]
  }, [publication])

  if (isPending || !publication) {
    return <div>{t("loading")}</div>
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
          categories={headerCategories}
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
            onLeaveReview={() => setReviewModalOpen(true)}
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
