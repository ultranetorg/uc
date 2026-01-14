import { memo, useMemo } from "react"

import { SvgStarXxs } from "assets"
import { ProductFieldModel } from "types"
import { ButtonPrimary, LinkFullscreen } from "ui/components"
import { ReviewsList } from "ui/components/specific"
import { AuthorImageTitle } from "ui/components/publication/SoftwareInfo/components"
import { buildFileUrl, formatAverageRating, formatDate } from "utils"
import { getValue, nameEq } from "ui/components/publication/utils"

import { ContentProps } from "../types"

type BookFieldMap = {
  author?: string
  publisher?: string
  isbn?: string
  publicationDate?: string
  genre?: string
  about?: string
  coverId?: string
}

function buildBookFields(fields: ProductFieldModel[] | undefined): BookFieldMap {
  if (!fields?.length) return {}

  return {
    author: getValue<string>(fields, "author"),
    publisher: getValue<string>(fields, "publisher"),
    isbn: getValue<string>(fields, "isbn"),
    publicationDate: getValue<string>(fields, "publication-date"),
    genre: getValue<string>(fields, "genre"),
    about: getValue<string>(fields, "about"),
    coverId: getValue<string>(fields, "cover-id"),
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

const LABEL_CLASSNAME = "leading-4 text-gray-500 text-2xs"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

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
    const publisherName = bookFields.publisher ?? publication.authorTitle

    return (
      <>
        {/* Left column: cover + download buttons */}
        <div className="flex w-80 flex-col gap-4">
          <div className="flex aspect-[3/4] w-full items-center justify-center overflow-hidden rounded-lg border border-gray-300 bg-gray-200">
            {coverSrc ? (
              <img src={coverSrc} alt={publication.title} className="h-full w-full object-cover" />
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

        {/* Right column: book info + reviews */}
        <div className="flex flex-1 flex-col gap-8">
          <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
            <div className="flex flex-col gap-4">
              <div className="flex flex-col gap-2">
                <span className={LABEL_CLASSNAME}>{t("publisher")}</span>
                <LinkFullscreen to={`/${siteId}/a/${publication.authorId}`}>
                  <AuthorImageTitle title={publication.authorTitle} authorAvatar={publication.authorAvatar} />
                </LinkFullscreen>
              </div>

              <div className="flex flex-col gap-2">
                <span className={LABEL_CLASSNAME}>{t("ratings")}</span>
                <span className="flex items-center gap-1 text-2sm font-medium leading-5 text-gray-800">
                  <span>{t("storeName")}: </span>
                  <span>{formatAverageRating(publication.rating)}</span>
                  <SvgStarXxs className="fill-favorite" />
                </span>
              </div>

              <div className="flex flex-col gap-2">
                <span className={LABEL_CLASSNAME}>{t("author")}</span>
                <span className={VALUE_CLASSNAME}>{authorName}</span>
              </div>

              <div className="flex flex-col gap-2">
                <span className={LABEL_CLASSNAME}>{t("publisher")}</span>
                <span className={VALUE_CLASSNAME}>{publisherName}</span>
              </div>

              {bookFields.isbn && (
                <div className="flex flex-col gap-2">
                  <span className={LABEL_CLASSNAME}>{t("isbn")}</span>
                  <span className={VALUE_CLASSNAME}>{bookFields.isbn}</span>
                </div>
              )}

              <div className="flex flex-col gap-2">
                <span className={LABEL_CLASSNAME}>{t("publicationDate")}</span>
                <span className={VALUE_CLASSNAME}>{publishedAt}</span>
              </div>

              {bookFields.genre && (
                <div className="flex flex-col gap-2">
                  <span className={LABEL_CLASSNAME}>{t("genre")}</span>
                  <span className={VALUE_CLASSNAME}>{bookFields.genre}</span>
                </div>
              )}
            </div>

            {aboutText && (
              <div className="mt-2 flex flex-col gap-2">
                <span className={LABEL_CLASSNAME}>{t("about")}</span>
                <p className="line-clamp-3 text-2sm leading-5 text-gray-800">{aboutText}</p>
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
