import { memo, useMemo } from "react"

import { SvgStarXxs } from "assets"
import { ProductFieldModel } from "types"
import { ButtonPrimary, LinkFullscreen } from "ui/components"
import { ReviewsList } from "ui/components/specific"
import { AuthorImageTitle } from "ui/components/publication/SoftwareInfo/components"
import { twMerge } from "tailwind-merge"
import { buildFileUrl, formatAverageRating } from "utils"
import { getValue, nameEq } from "ui/components/publication/utils"

import { ContentProps } from "../types"

type MovieFieldMap = {
  director?: string
  writer?: string
  stars?: string
  time?: string
  releaseDate?: string
  countries?: string
  genre?: string
  quality?: string
  translation?: string
  imdb?: string
  posterId?: string
  about?: string
}

function buildMovieFields(fields: ProductFieldModel[] | undefined): MovieFieldMap {
  if (!fields?.length) return {}

  return {
    director: getValue<string>(fields, "director"),
    writer: getValue<string>(fields, "writer"),
    stars: getValue<string>(fields, "stars"),
    time: getValue<string>(fields, "time"),
    releaseDate: getValue<string>(fields, "release-date"),
    countries: getValue<string>(fields, "countries"),
    genre: getValue<string>(fields, "genre"),
    quality: getValue<string>(fields, "quality"),
    translation: getValue<string>(fields, "translation"),
    imdb: getValue<string>(fields, "imdb"),
    posterId: getValue<string>(fields, "cover-id"),
    about: getValue<string>(fields, "about"),
  }
}

function getMaxDescription(fields: ProductFieldModel[] | undefined): string | undefined {
  if (!fields?.length) return undefined

  const nodes = fields.filter(f => nameEq(f.name, "description-maximal"))
  if (!nodes.length) return undefined

  // Prefer English description if present
  for (const node of nodes) {
    const children = node.children ?? []
    const lang = getValue<string>(children, "language")
    const text =
      getValue<string>(children, "value") ?? getValue<string>(children, "description") ?? undefined

    if ((lang ?? "").toLowerCase() === "en" && text) {
      return text
    }
  }

  // Fallback: first node with any text
  for (const node of nodes) {
    const children = node.children ?? []
    const text =
      getValue<string>(children, "value") ?? getValue<string>(children, "description") ?? undefined
    if (text) return text
  }

  return undefined
}

const LABEL_CLASSNAME = "w-28 text-2sm font-medium leading-5 text-gray-900"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm leading-5 text-gray-900"

export const MoviePublicationContent = memo(
  ({ t, siteId, publication, isPending, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const fields = publication.productFields

    const movieFields = useMemo(() => buildMovieFields(fields), [fields])

    const posterSrc = useMemo(() => {
      if (!movieFields.posterId) return undefined
      return buildFileUrl(movieFields.posterId)
    }, [movieFields.posterId])

    const aboutText = useMemo(() => {
      const fromFields = movieFields.about ?? getMaxDescription(fields)
      return fromFields ?? publication.description
    }, [movieFields.about, fields, publication.description])

    const publisherAccountName = publication.authorTitle

    return (
      <>
        {/* Left column: poster + download buttons */}
        <div className="flex w-80 flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6">
          <div className="flex aspect-[3/4] w-full items-center justify-center overflow-hidden rounded-lg bg-gray-200">
            {posterSrc ? (
              <img src={posterSrc} alt={publication.title} className="h-full w-full object-cover" />
            ) : (
              <div className="h-full w-full bg-gray-300" />
            )}
          </div>

          <div className="flex flex-col gap-3">
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
                {movieFields.imdb && (
                  <span className="text-2sm leading-5 text-gray-900">IMDb: {movieFields.imdb}</span>
                )}
              </span>
            </div>

            {/* Director */}
            <div className="flex gap-6 py-1">
              <span className={LABEL_CLASSNAME}>{t("director")}:</span>
              <span className={VALUE_CLASSNAME}>{movieFields.director}</span>
            </div>

            {/* Writer */}
            <div className="flex gap-6 py-1">
              <span className={LABEL_CLASSNAME}>{t("writer")}:</span>
              <span className={VALUE_CLASSNAME}>{movieFields.writer}</span>
            </div>

            {/* Stars */}
            <div className="flex gap-6 py-1">
              <span className={LABEL_CLASSNAME}>{t("stars")}:</span>
              {movieFields.stars && (
                <p className="text-2sm leading-5 text-gray-900">{movieFields.stars}</p>
              )}
            </div>

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
              <div className="flex gap-6 py-1">
                <span className={LABEL_CLASSNAME}>{t("about")}:</span>
                <p className="line-clamp-3 text-2sm leading-5 text-gray-900">{aboutText}</p>
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
