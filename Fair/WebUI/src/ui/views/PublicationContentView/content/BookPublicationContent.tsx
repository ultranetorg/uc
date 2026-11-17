import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { LinkFullscreen } from "ui/components"
import { AuthorImageTitle } from "ui/components/publication/SoftwareInfo/components"
import { ReviewsList } from "ui/components/specific"
import { buildFileUrl, formatAverageRating, formatDate, routes } from "utils"

import { ContentProps } from "../types"

import { buildBookFields, getMaxDescription } from "./utils"

const LABEL_CLASSNAME = "w-28 shrink-0 text-2xs font-medium leading-4 sm:w-40"
const VALUE_CLASSNAME = "min-w-0 truncate text-2sm leading-5"
const LONG_VALUE_CLASSNAME = "line-clamp-3 text-2sm leading-5"

export const BookPublicationContent = memo(
  ({
    t,
    storeId,
    productOrPublication,
    isReviewsLoading,
    reviews,
    error,
    onLeaveReview,
    onShowMore,
    hasMoreReviews,
  }: ContentProps) => {
    const fields = productOrPublication.fields

    const bookFields = useMemo(() => buildBookFields(fields), [fields])

    const coverSrc = useMemo(() => {
      if (!bookFields.coverId) return undefined
      return buildFileUrl(bookFields.coverId)
    }, [bookFields.coverId])

    const aboutText = useMemo(() => bookFields.about ?? getMaxDescription(fields), [bookFields.about, fields])

    const publishedAt = useMemo(() => {
      if (bookFields.publicationDate) return bookFields.publicationDate
      return formatDate(productOrPublication.updated)
    }, [bookFields.publicationDate, productOrPublication.updated])

    const authorName = bookFields.author ?? productOrPublication.authorTitle
    const authorTitle = productOrPublication.authorTitle
    const publisherName = bookFields.publisher ?? productOrPublication.authorTitle

    return (
      <>
        {/* Left column: cover + download buttons */}
        <div className="flex h-fit w-full flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-4 sm:p-6 lg:w-87.5 lg:shrink-0">
          <div className="flex h-96 items-center justify-center overflow-hidden rounded-lg bg-gray-200 sm:h-112.5">
            {coverSrc ? (
              <img src={coverSrc} alt={productOrPublication.title} className="size-full object-cover" />
            ) : (
              <div className="size-full bg-gray-500" />
            )}
          </div>
        </div>

        <div className="flex min-w-0 flex-1 flex-col gap-6 lg:max-w-187.5 lg:gap-8">
          <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-4 sm:p-6">
            {/* Publisher (account with avatar) */}
            <div className="flex items-center gap-3 sm:gap-4">
              <span className={LABEL_CLASSNAME}>{t("publisher")}:</span>
              <LinkFullscreen className="min-w-0" to={routes.publisher(storeId, productOrPublication.authorId)}>
                <AuthorImageTitle title={authorTitle} authorFileId={productOrPublication.authorId} />
              </LinkFullscreen>
            </div>

            {/* Ratings */}
            {"rating" in productOrPublication && productOrPublication.rating !== undefined && (
              <div className="flex items-center gap-3 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("labels:rating")}</span>
                <span className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1 whitespace-nowrap")}>
                  <span className="font-semibold">{formatAverageRating(productOrPublication.rating)}</span>
                  <SvgStarXxs className="fill-favorite" />
                </span>
              </div>
            )}

            {/* Author */}
            <div className="flex gap-3 sm:gap-6">
              <span className={LABEL_CLASSNAME}>{t("author")}:</span>
              <span className={VALUE_CLASSNAME}>{authorName}</span>
            </div>

            {/* Publisher (book publisher) */}
            <div className="flex gap-3 sm:gap-6">
              <span className={LABEL_CLASSNAME}>{t("publisher")}:</span>
              <span className={VALUE_CLASSNAME}>{publisherName}</span>
            </div>

            {/* ISBN */}
            {bookFields.isbn && (
              <div className="flex gap-3 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("isbn")}:</span>
                <span className={VALUE_CLASSNAME}>{bookFields.isbn}</span>
              </div>
            )}

            {/* Publication Date */}
            <div className="flex gap-3 sm:gap-6">
              <span className={LABEL_CLASSNAME}>{t("publicationDate")}:</span>
              <span className={VALUE_CLASSNAME}>{publishedAt}</span>
            </div>

            {/* Genre */}
            {bookFields.genre && (
              <div className="flex gap-3 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("genre")}:</span>
                <span className={VALUE_CLASSNAME}>{bookFields.genre}</span>
              </div>
            )}

            {/* About */}
            {aboutText && (
              <div className="flex flex-col gap-2">
                <span className={LABEL_CLASSNAME}>{t("about")}:</span>
                <span className={LONG_VALUE_CLASSNAME}>{aboutText}</span>
              </div>
            )}
          </div>

          {(reviews || isReviewsLoading) && onLeaveReview && (
            <ReviewsList
              isLoading={!!isReviewsLoading}
              reviews={reviews}
              error={error}
              onLeaveReviewClick={onLeaveReview}
              leaveReviewLabel={t("writeReview")}
              noReviewsLabel={t("noReviews")}
              reviewLabel={t("review", { count: reviews?.totalItems })}
              showMoreReviewsLabel={t("showMoreReviews")}
              onShowMore={onShowMore}
              hasMoreReviews={hasMoreReviews}
            />
          )}
        </div>
      </>
    )
  },
)
