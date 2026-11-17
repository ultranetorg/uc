import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { LinkFullscreen } from "ui/components"
import { ReviewsList } from "ui/components/specific"
import { AuthorImageTitle } from "ui/components/publication/SoftwareInfo/components"
import { buildFileUrl, formatAverageRating, routes } from "utils"

import { ContentProps } from "../types"

import { buildVideoFields, getMaxDescription } from "./utils"

const LABEL_CLASSNAME = "w-28 shrink-0 text-2xs font-medium leading-4 sm:w-40"
const VALUE_CLASSNAME = "min-w-0 truncate text-2sm leading-5"
const LONG_VALUE_CLASSNAME = "line-clamp-3 text-2sm leading-5"

export const VideoPublicationContent = memo(
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

    const videoFields = useMemo(() => buildVideoFields(fields), [fields])

    const posterSrc = useMemo(() => buildFileUrl(videoFields.posterId), [videoFields.posterId])

    const aboutText = useMemo(() => videoFields.about ?? getMaxDescription(fields), [videoFields.about, fields])

    const publisherAccountName = productOrPublication.authorTitle

    return (
      <>
        {/* Left column: poster + download buttons */}
        <div className="flex h-fit w-full flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-4 sm:p-6 lg:w-87.5 lg:shrink-0">
          <div className="flex h-96 items-center justify-center overflow-hidden rounded-lg bg-gray-200 sm:h-112.5">
            {posterSrc ? (
              <img src={posterSrc} alt={productOrPublication.title} className="size-full object-cover" />
            ) : (
              <div className="size-full bg-gray-300" />
            )}
          </div>

          {/* <div className="flex flex-col gap-4">
            <ButtonPrimary className="w-full" label={t("downloadFromRdn")} />
            <ButtonPrimary className="w-full" label={t("downloadFromTorrent")} />
            <ButtonPrimary className="w-full" label={t("downloadFromIpfs")} />
          </div> */}
        </div>

        {/* Right column: movie info + reviews */}
        <div className="flex min-w-0 flex-1 flex-col gap-6 lg:gap-8">
          <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-4 sm:p-6">
            {/* Publisher (account with avatar) */}
            <div className="flex items-center gap-3 py-1 sm:gap-6">
              <span className={LABEL_CLASSNAME}>{t("publisher")}:</span>
              <LinkFullscreen className="min-w-0" to={routes.publisher(storeId, productOrPublication.authorId)}>
                <AuthorImageTitle title={publisherAccountName} authorFileId={productOrPublication.authorId} />
              </LinkFullscreen>
            </div>

            {/* Ratings */}
            {"rating" in productOrPublication && productOrPublication.rating !== undefined && (
              <div className="flex items-center gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("labels:rating")}</span>
                <span className={twMerge(VALUE_CLASSNAME, "flex flex-wrap items-center gap-3 whitespace-nowrap")}>
                  <span className="flex items-center gap-1">
                    {productOrPublication.rating > 0 ? (
                      <>
                        <span className="font-semibold">{formatAverageRating(productOrPublication.rating)}</span>
                        <SvgStarXxs className="fill-favorite" />
                      </>
                    ) : (
                      "N/A"
                    )}
                  </span>
                  {videoFields.imdb && (
                    <span className="text-2sm leading-5 text-gray-900">IMDb: {videoFields.imdb}</span>
                  )}
                </span>
              </div>
            )}

            {/* Director */}
            {videoFields.director && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("director")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.director}</span>
              </div>
            )}

            {/* Writer */}
            {videoFields.writer && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("writer")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.writer}</span>
              </div>
            )}

            {/* Stars */}
            {videoFields.stars && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("stars")}:</span>
                {videoFields.stars && <p className="min-w-0 text-2sm leading-5 text-gray-900">{videoFields.stars}</p>}
              </div>
            )}

            {/* Time */}
            {videoFields.time && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("time")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.time}</span>
              </div>
            )}

            {/* Release date */}
            {videoFields.releaseDate && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("releaseDate")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.releaseDate}</span>
              </div>
            )}

            {/* Countries */}
            {videoFields.countries && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("countries")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.countries}</span>
              </div>
            )}

            {/* Genre */}
            {videoFields.genre && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("genre")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.genre}</span>
              </div>
            )}

            {/* Quality */}
            {videoFields.quality && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("quality")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.quality}</span>
              </div>
            )}

            {/* Translation */}
            {videoFields.translation && (
              <div className="flex gap-3 py-1 sm:gap-6">
                <span className={LABEL_CLASSNAME}>{t("translation")}:</span>
                <span className={VALUE_CLASSNAME}>{videoFields.translation}</span>
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
