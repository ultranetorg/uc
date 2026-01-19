import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { ButtonPrimary, LinkFullscreen } from "ui/components"
import { ReviewsList } from "ui/components/specific"
import { AuthorImageTitle } from "ui/components/publication/SoftwareInfo/components"
import { buildFileUrl, formatAverageRating } from "utils"

import { ContentProps } from "../types"

import { buildMovieFields, getMaxDescription } from "./utils"

const LABEL_CLASSNAME = "w-40 text-2xs font-medium leading-4"
const VALUE_CLASSNAME = "truncate text-2sm leading-5"
const LONG_VALUE_CLASSNAME = "line-clamp-3 text-2sm leading-5"

export const MoviePublicationContent = memo(
  ({ t, siteId, publication, isPending, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const fields = publication.productFields

    const movieFields = useMemo(() => buildMovieFields(fields), [fields])

    const posterSrc = useMemo(() => buildFileUrl(movieFields.posterId), [movieFields.posterId])

    const aboutText = useMemo(
      () => movieFields.about ?? getMaxDescription(fields) ?? publication.description,
      [movieFields.about, fields, publication.description],
    )

    const publisherAccountName = publication.authorTitle

    return (
      <>
        {/* Left column: poster + download buttons */}
        <div className="flex h-fit w-87.5 flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6">
          <div className="flex h-112.5 items-center justify-center overflow-hidden rounded-lg bg-gray-200">
            {posterSrc ? (
              <img src={posterSrc} alt={publication.title} className="size-full object-cover" />
            ) : (
              <div className="size-full bg-gray-300" />
            )}
          </div>

          <div className="flex flex-col gap-4">
            <ButtonPrimary className="w-full" label={t("downloadFromRdn")} />
            <ButtonPrimary className="w-full" label={t("downloadFromTorrent")} />
            <ButtonPrimary className="w-full" label={t("downloadFromTorrent")} />
            <ButtonPrimary className="w-full" label={t("downloadFromWeb")} />
          </div>
        </div>

        {/* Right column: movie info + reviews */}
        <div className="flex flex-1 flex-col gap-8">
          <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6">
            {/* Publisher (account with avatar) */}
            <div className="flex items-center gap-6 py-1">
              <span className={LABEL_CLASSNAME}>{t("publisher")}:</span>
              <LinkFullscreen to={`/${siteId}/a/${publication.authorId}`}>
                <AuthorImageTitle title={publisherAccountName} authorAvatar={publication.authorAvatar} />
              </LinkFullscreen>
            </div>

            {/* Ratings */}
            <div className="flex items-center gap-6 py-1">
              <span className={LABEL_CLASSNAME}>{t("ratings")}:</span>
              <span className={twMerge(VALUE_CLASSNAME, "flex flex-wrap items-center gap-3 whitespace-nowrap")}>
                <span className="flex items-center gap-1">
                  <span>{t("storeName")}: </span>
                  <span className="font-semibold">{formatAverageRating(publication.rating)}</span>
                  <SvgStarXxs className="fill-favorite" />
                </span>
                {movieFields.imdb && <span className="text-2sm leading-5 text-gray-900">IMDb: {movieFields.imdb}</span>}
              </span>
            </div>

            {/* Director */}
            {movieFields.director && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("director")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.director}</span>
              </div>
            )}

            {/* Writer */}
            {movieFields.writer && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("writer")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.writer}</span>
              </div>
            )}

            {/* Stars */}
            {movieFields.stars && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("stars")}:</span>
                {movieFields.stars && <p className="text-2sm leading-5 text-gray-900">{movieFields.stars}</p>}
              </div>
            )}

            {/* Time */}
            {movieFields.time && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("time")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.time}</span>
              </div>
            )}

            {/* Release date */}
            {movieFields.releaseDate && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("releaseDate")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.releaseDate}</span>
              </div>
            )}

            {/* Countries */}
            {movieFields.countries && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("countries")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.countries}</span>
              </div>
            )}

            {/* Genre */}
            {movieFields.genre && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("genre")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.genre}</span>
              </div>
            )}

            {/* Quality */}
            {movieFields.quality && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("quality")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.quality}</span>
              </div>
            )}

            {/* Translation */}
            {movieFields.translation && (
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("translation")}:</span>
                <span className={VALUE_CLASSNAME}>{movieFields.translation}</span>
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
