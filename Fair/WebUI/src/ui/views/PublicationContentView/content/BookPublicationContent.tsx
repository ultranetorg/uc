import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { ButtonPrimary, LinkFullscreen } from "ui/components"
import { AuthorImageTitle } from "ui/components/publication/SoftwareInfo/components"
import { ReviewsList } from "ui/components/specific"
import { buildFileUrl, formatAverageRating, formatDate } from "utils"

import { ContentProps } from "../types"

import { buildBookFields, getMaxDescription } from "./utils"

const LABEL_CLASSNAME = "w-40 text-2xs font-medium leading-4"
const VALUE_CLASSNAME = "truncate text-2sm leading-5"
const LONG_VALUE_CLASSNAME = "line-clamp-3 text-2sm leading-5"

export const BookPublicationContent = memo(
  ({ t, siteId, productOrPublication, isPending, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const fields = productOrPublication.productFields

    const bookFields = useMemo(() => buildBookFields(fields), [fields])

    const coverSrc = useMemo(() => {
      if (!bookFields.coverId) return undefined
      return buildFileUrl(bookFields.coverId)
    }, [bookFields.coverId])

    const aboutText = useMemo(() => {
      const fromFields = bookFields.about ?? getMaxDescription(fields)
      return fromFields ?? productOrPublication.description
    }, [bookFields.about, fields, productOrPublication.description])

    const publishedAt = useMemo(() => {
      if (bookFields.publicationDate) return bookFields.publicationDate
      return formatDate(productOrPublication.updated)
    }, [bookFields.publicationDate, productOrPublication.updated])

    const authorName = bookFields.author ?? productOrPublication.authorTitle
    const publisherAccountName = productOrPublication.authorTitle
    const publisherName = bookFields.publisher ?? productOrPublication.authorTitle

    return (
      <>
        {/* Left column: cover + download buttons */}
        <div className="flex h-fit w-87.5 flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
          <div className="flex h-112.5 items-center justify-center overflow-hidden rounded-lg bg-gray-200">
            {coverSrc ? (
              <img src={coverSrc} alt={productOrPublication.title} className="size-full object-cover" />
            ) : (
              <div className="size-full bg-gray-500" />
            )}
          </div>

          <div className="flex flex-col gap-4">
            <ButtonPrimary className="w-full" label={t("downloadFromRdn")} />
            <ButtonPrimary className="w-full" label={t("downloadFromTorrent")} />
            <ButtonPrimary className="w-full" label={t("downloadFromIpfs")} />
          </div>
        </div>

        {/* Right column: book info + reviews */}
        <div className="flex max-w-187.5 flex-1 flex-col gap-8">
          <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6">
            {/* Publisher (account with avatar) */}
            <div className="flex items-center gap-4">
              <span className={LABEL_CLASSNAME}>{t("publisher")}:</span>
              <LinkFullscreen to={`/${siteId}/a/${productOrPublication.authorId}`}>
                <AuthorImageTitle title={publisherAccountName} authorFileId={productOrPublication.authorId} />
              </LinkFullscreen>
            </div>

            {/* Ratings */}
            {"rating" in productOrPublication && (
              <div className="flex items-center gap-6">
                <span className={LABEL_CLASSNAME}>{t("ratings")}:</span>
                <span className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1 whitespace-nowrap")}>
                  <span className="font-semibold">{formatAverageRating(productOrPublication.rating)}</span>
                  <SvgStarXxs className="fill-favorite" />
                </span>
              </div>
            )}

            {/* Author */}
            <div className="flex gap-6">
              <span className={LABEL_CLASSNAME}>{t("author")}:</span>
              <span className={VALUE_CLASSNAME}>{authorName}</span>
            </div>

            {/* Publisher (book publisher) */}
            <div className="flex gap-6">
              <span className={LABEL_CLASSNAME}>{t("publisher")}:</span>
              <span className={VALUE_CLASSNAME}>{publisherName}</span>
            </div>

            {/* ISBN */}
            {bookFields.isbn && (
              <div className="flex gap-6">
                <span className={LABEL_CLASSNAME}>{t("isbn")}:</span>
                <span className={VALUE_CLASSNAME}>{bookFields.isbn}</span>
              </div>
            )}

            {/* Publication Date */}
            <div className="flex gap-6">
              <span className={LABEL_CLASSNAME}>{t("publicationDate")}:</span>
              <span className={VALUE_CLASSNAME}>{publishedAt}</span>
            </div>

            {/* Genre */}
            {bookFields.genre && (
              <div className="flex gap-6">
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

          {reviews && onLeaveReview && (
            <ReviewsList
              isPending={isPending || isPendingReviews!}
              reviews={reviews}
              error={error}
              onLeaveReviewClick={onLeaveReview}
              leaveReviewLabel={t("leaveReview")}
              noReviewsLabel={t("noReviews")}
              reviewLabel={t("review", { count: reviews?.totalItems })}
              showMoreReviewsLabel={t("showMoreReviews")}
            />
          )}
        </div>
      </>
    )
  },
)
