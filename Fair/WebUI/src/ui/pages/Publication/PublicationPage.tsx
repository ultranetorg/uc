import { useCallback, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { useGetPublication, useGetReviews } from "entities"
import { ReviewsList } from "ui/components/specific"
import { Description, ReviewModal, SiteLink, Slider, SoftwareInfo } from "ui/components/specific/publication"

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

  const handleLeaveReviewClick = useCallback(() => setReviewModalOpen(true), [])
  const handleReviewModalClose = useCallback(() => setReviewModalOpen(false), [])
  const handleReviewModalSubmit = useCallback(() => {
    console.log("handleReviewModalSubmit")
    setReviewModalOpen(false)
  }, [])

  if (isPending || !publication) {
    return <div>Loading</div>
  }

  return (
    <>
      <div className="flex gap-8">
        <div className="flex flex-1 flex-col gap-8">
          <Slider />
          <Description
            text={publication.description}
            showMoreLabel={t("showMore")}
            descriptionLabel={t("information")}
          />
          <ReviewsList
            isPending={isPending || isPendingReviews}
            reviews={reviews}
            error={error}
            onLeaveReviewClick={handleLeaveReviewClick}
            leaveReviewLabel={t("leaveReview")}
            noReviewsLabel={t("noReviews")}
            reviewLabel={t("review", { count: reviews?.totalItems })}
            showMoreReviewsLabel={t("showMoreReviews")}
          />
        </div>

        <div className="flex w-87.5 flex-col gap-8">
          <SoftwareInfo
            publication={publication}
            publisherLabel={t("publisher")}
            versionLabel={t("version")}
            activationLabel={t("activation")}
            osLabel={t("os")}
            ratingLabel={t("rating")}
            lastUpdatedLabel={t("lastUpdated")}
          />
          <SiteLink to={"google.com"} label={t("officialSite")} />
        </div>
      </div>
      {isReviewModalOpen && (
        <ReviewModal
          isOpen={isReviewModalOpen}
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
