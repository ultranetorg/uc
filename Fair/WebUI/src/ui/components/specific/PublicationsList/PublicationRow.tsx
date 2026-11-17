import { memo } from "react"

import { Trans } from "react-i18next"
import { SvgStarXxs } from "assets"
import { SvgSoftwareLogo3Xl } from "assets/fallback"
import { Publication, PublicationExtended } from "types"
import { ImageFallback } from "ui/components"
import { PublicationCardContextThreeDotsMenu } from "ui/components/specific"
import { buildFileUrl, formatAverageRating } from "utils"

export type PublicationRowBaseProps = { showPublicationType?: boolean }

export type PublicationRowProps = Publication &
  Partial<Pick<PublicationExtended, "authorTitle" | "type">> &
  PublicationRowBaseProps

export const PublicationRow = memo(
  ({ id, title, logoFileId, authorTitle, categoryTitle, rating, type, showPublicationType }: PublicationRowProps) => {
    const formattedRating = formatAverageRating(rating)

    return (
      <div
        className="flex cursor-pointer items-center gap-3 bg-gray-100 p-3 text-2sm leading-5 text-gray-900 hover:bg-gray-200 sm:gap-4 sm:p-4"
        title={title}
      >
        <div className="flex max-w-130 min-w-0 flex-2 items-center gap-2">
          <div className="size-8 flex-none overflow-hidden rounded-lg bg-gray-500">
            <ImageFallback
              src={buildFileUrl(logoFileId)}
              fallback={<SvgSoftwareLogo3Xl className="size-full object-cover" />}
            />
          </div>
          <div className="flex min-w-0 flex-col">
            <span className="truncate font-medium">{title}</span>
            <span className="truncate text-2xs leading-4 text-gray-500 md:hidden">
              {[authorTitle, categoryTitle].filter(Boolean).join(" · ")}
            </span>
          </div>
        </div>
        {showPublicationType && (
          <Trans ns="categoryTypes" i18nKey={type} parent="span" className="hidden w-24 flex-none truncate md:block">
            {type}
          </Trans>
        )}
        <span className="hidden max-w-60 min-w-0 flex-1 truncate md:block">{categoryTitle}</span>
        <span className="hidden max-w-60 min-w-0 flex-1 truncate md:block">{authorTitle}</span>
        <span className="flex w-12 flex-none items-center sm:w-16" title={formattedRating}>
          {rating > 0 && (
            <>
              {formattedRating} <SvgStarXxs className="fill-favorite" />
            </>
          )}
        </span>

        <PublicationCardContextThreeDotsMenu
          publicationId={id}
          publicationTitle={title}
          size="large"
          className="w-8 flex-none"
        />
      </div>
    )
  },
)
