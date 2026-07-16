import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useSiteRolesContext } from "app"
import { SvgStarXxs } from "assets"
import { SvgSoftwareLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { ModeratorPublicationContextMenu } from "ui/components/specific"
import { buildFileUrl, formatRating } from "utils"

import { PublicationCardProps } from "./types"

export const DefaultPublicationCard = memo(
  ({ id, title, logoFileId, authorTitle, categoryTitle, rating }: PublicationCardProps) => {
    const { isModerator } = useSiteRolesContext()

    return (
      <div
        className="relative flex w-67.75 flex-col items-center justify-center gap-4 rounded-lg bg-gray-100 p-4 hover:bg-gray-200"
        title={title}
      >
        <div className="size-14 overflow-hidden rounded-lg">
          <ImageFallback
            src={buildFileUrl(logoFileId)}
            fallback={<SvgSoftwareLogo className="size-full object-cover" />}
          />
        </div>
        <div className="flex w-40 flex-col gap-1 text-center">
          <span className="truncate text-2sm font-medium leading-4.5 text-gray-800">{title}</span>
          <span className="truncate text-2xs leading-3.75 text-gray-500">{authorTitle}</span>
          <span className="truncate text-2xs leading-3.75 text-gray-500">{categoryTitle}</span>
        </div>

        {rating > 0 && (
          <span className={twMerge("absolute top-2.5 flex items-center", !isModerator ? "right-2.5" : "left-2.5")}>
            {formatRating(rating)} <SvgStarXxs className="fill-favorite" />
          </span>
        )}
        {isModerator && (
          <ModeratorPublicationContextMenu
            publicationId={id}
            publicationTitle={title}
            className="absolute right-1 top-1"
          />
        )}
      </div>
    )
  },
)
