import { memo } from "react"

import { SvgStarXxs } from "assets"
import { SvgSoftwareLogo } from "assets/fallback"
import { Publication, PublicationExtended } from "types"
import { ImageFallback } from "ui/components"
import { ModeratorPublicationContextMenu } from "ui/components/specific"
import { buildFileUrl, formatRating } from "utils"

export type PublicationRowProps = Publication & Partial<Pick<PublicationExtended, "authorTitle">>

export const PublicationRow = memo(
  ({ id, title, logoFileId, authorTitle, categoryTitle, rating }: PublicationRowProps) => {
    const formattedRating = formatRating(rating)

    return (
      <div
        className="flex cursor-pointer items-center justify-between gap-6 bg-gray-100 p-4 text-2sm leading-5 text-gray-900 hover:bg-gray-200"
        title={title}
      >
        <div className="flex w-5/12 max-w-[520px] items-center gap-2">
          <div className="size-8 flex-none overflow-hidden rounded-lg bg-gray-500">
            <ImageFallback
              src={buildFileUrl(logoFileId)}
              fallback={<SvgSoftwareLogo className="size-full object-cover" />}
            />
          </div>
          <span className="truncate font-medium">{title}</span>
        </div>
        {authorTitle && <span className="w-1/4 max-w-60 truncate">{authorTitle}</span>}
        <span className="w-1/4 max-w-60 truncate">{categoryTitle}</span>
        <span className="flex w-1/12 items-center" title={formattedRating}>
          {rating > 0 && (
            <>
              {formattedRating} <SvgStarXxs className="fill-favorite" />
            </>
          )}
        </span>

        <ModeratorPublicationContextMenu publicationId={id} publicationTitle={title} size="large" />
      </div>
    )
  },
)
