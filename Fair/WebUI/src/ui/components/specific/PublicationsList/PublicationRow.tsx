import { memo } from "react"

import { Trans } from "react-i18next"
import { useStoreRolesContext } from "app"
import { SvgStarXxs } from "assets"
import { SvgSoftwareLogo } from "assets/fallback"
import { Publication, PublicationExtended } from "types"
import { ImageFallback } from "ui/components"
import { ModeratorPublicationContextMenu } from "ui/components/specific"
import { buildFileUrl, formatAverageRating } from "utils"

export type PublicationRowBaseProps = { showPublicationType?: boolean }

export type PublicationRowProps = Publication &
  Partial<Pick<PublicationExtended, "authorTitle" | "type">> &
  PublicationRowBaseProps

export const PublicationRow = memo(
  ({ id, title, logoFileId, authorTitle, categoryTitle, rating, type, showPublicationType }: PublicationRowProps) => {
    const { isModerator } = useStoreRolesContext()

    const formattedRating = formatAverageRating(rating)

    return (
      <div
        className="flex cursor-pointer items-center gap-4 bg-gray-100 p-4 text-2sm leading-5 text-gray-900 hover:bg-gray-200"
        title={title}
      >
        <div className="flex min-w-0 max-w-[520px] flex-[2] items-center gap-2">
          <div className="size-8 flex-none overflow-hidden rounded-lg bg-gray-500">
            <ImageFallback
              src={buildFileUrl(logoFileId)}
              fallback={<SvgSoftwareLogo className="size-full object-cover" />}
            />
          </div>
          <span className="truncate font-medium">{title}</span>
        </div>
        {showPublicationType && (
          <Trans ns="categoryTypes" i18nKey={type} parent="span" className="w-24 flex-none truncate">
            {type}
          </Trans>
        )}
        <span className="min-w-0 max-w-60 flex-1 truncate">{categoryTitle}</span>
        <span className="min-w-0 max-w-60 flex-1 truncate">{authorTitle}</span>
        <span className="flex w-16 flex-none items-center" title={formattedRating}>
          {rating > 0 && (
            <>
              {formattedRating} <SvgStarXxs className="fill-favorite" />
            </>
          )}
        </span>

        {isModerator && (
          <ModeratorPublicationContextMenu
            publicationId={id}
            publicationTitle={title}
            size="large"
            className="w-8 flex-none"
          />
        )}
      </div>
    )
  },
)
