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
  ({ t, siteId, publication, isPending, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const fields = publication.productFields

    const bookFields = useMemo(() => buildBookFields(fields), [fields])

    const coverSrc = useMemo(() => {
      if (!bookFields.coverId) return undefined
      return buildFileUrl(bookFields.coverId)
    }, [bookFields.coverId])

    const aboutText = useMemo(() => {
      const fromFields = bookFields.about ?? getMaxDescription(fields)
      return fromFields ?? publication.description
    }, [bookFields.about, fields, publication.description])

    const publishedAt = useMemo(() => {
      if (bookFields.publicationDate) return bookFields.publicationDate
      return formatDate(publication.productUpdated)
    }, [bookFields.publicationDate, publication.productUpdated])

    const authorName = bookFields.author ?? publication.authorTitle
    const publisherAccountName = publication.authorTitle
    const publisherName = bookFields.publisher ?? publication.authorTitle

    return (
      <>
        {/* Left column: cover + download buttons */}
        <div className="flex w-87.5 flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
          <div className="flex aspect-[3/4] w-full items-center justify-center overflow-hidden rounded-lg bg-gray-200">
            {coverSrc ? (
              <img src={coverSrc} alt={publication.title} className="size-full object-cover" />
            ) : (
              <div className="size-full bg-gray-500" />
            )}
          </div>

          <div className="flex flex-col gap-4">
            <ButtonPrimary className="w-full" label={t("downloadFromRdn")} />
            <ButtonPrimary className="w-full" label={t("downloadFromTorrent")} />
            <ButtonPrimary className="w-full" label={t("downloadFromTorrent")} />
            <ButtonPrimary className="w-full" label={t("downloadFromWeb")} />
          </div>
        </div>

        {/* Right column: book info + reviews */}
        <div className="flex max-w-187.5 flex-1 flex-col gap-8">
          <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6">
            {/* Publisher (account with avatar) */}
            <div className="flex items-center gap-4">
              <span className={LABEL_CLASSNAME}>{t("publisher")}:</span>
              <LinkFullscreen to={`/${siteId}/a/${publication.authorId}`}>
                <AuthorImageTitle title={publisherAccountName} authorAvatar={publication.authorAvatar} />
              </LinkFullscreen>
            </div>

            {/* Ratings */}
            <div className="flex items-center gap-6">
              <span className={LABEL_CLASSNAME}>{t("ratings")}:</span>
              <span className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1 whitespace-nowrap")}>
                <span className="font-semibold">{formatAverageRating(publication.rating)}</span>
                <SvgStarXxs className="fill-favorite" />
              </span>
            </div>

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

          <ReviewsList
            isPending={isPending || isPendingReviews}
            reviews={reviews}
            error={error}
            onLeaveReviewClick={onLeaveReview}
            leaveReviewLabel={t("leaveReview")}
            noReviewsLabel={t("noReviews")}
            reviewLabel={t("review", { count: reviews?.totalItems })}
            showMoreReviewsLabel={t("showMoreReviews")}
          />
        </div>
      </>
    )
  },
)
