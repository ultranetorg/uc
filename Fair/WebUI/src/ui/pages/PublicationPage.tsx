import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { useGetPublication, useGetReviews } from "entities"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import {
  Description,
  ReviewModal,
  SiteLink,
  Slider,
  SoftwareInfo,
  SoftwarePublicationHeader,
  SystemRequirementsTabs,
} from "ui/components/publication"
import { ReviewsList } from "ui/components/specific"
import { TEST_SOFTWARE_CATEGORIES } from "testConfig"
import { createBreadcrumbs } from "utils"

const TEST_TAB_ITEMS = [
  {
    key: "windows",
    label: "Windows",
    sections: [
      {
        key: "Minimum",
        name: "Minimum",
        values: {
          CPU: "AMD Ryzen 5 1600 or Intel Core i5 6600K",
          RAM: "8 GB",
          "Video Card": "AMD Radeon RX 570 or Nvidia GeForce GTX 1050 Ti",
          "Dedicated Video RAM": "4096 MB",
          OS: "Windows 10 / 11 - 64-Bit (Latest Update)",
          "Free Disk Space": "100 GB",
        },
      },
      {
        key: "recommended",
        name: "Recommended",
        values: {
          CPU: "AMD Ryzen 7 2700X or Intel Core i7 6700",
          RAM: "12 GB",
          "Video Card": "AMD Radeon RX 570 or Nvidia GeForce GTX 1050 Ti",
          "Dedicated Video RAM": "4096 MB",
          OS: "Windows 10 / 11 - 64-Bit (Latest Update)",
          "Free Disk Space": "100 GB",
        },
      },
    ],
  },
  { key: "linux", label: "Linux", sections: [] },
  { key: "macos", label: "macOS", sections: [] },
]

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

  const handleLeaveReviewClick = useCallback(() => setReviewModalOpen(true), [])
  const handleReviewModalClose = useCallback(() => setReviewModalOpen(false), [])
  const handleReviewModalSubmit = useCallback(() => setReviewModalOpen(false), [])

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
        <div className="flex gap-8">
          <div className="flex flex-1 flex-col gap-8">
            <Slider />
            <Description
              text={publication.description}
              showMoreLabel={t("showMore")}
              descriptionLabel={t("information")}
            />
            <SystemRequirementsTabs label={t("systemRequirements")} tabs={TEST_TAB_ITEMS} />
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
              siteId={siteId!}
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
