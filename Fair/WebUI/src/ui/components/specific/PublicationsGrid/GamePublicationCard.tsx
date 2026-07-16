import { memo } from "react"

import { useSiteRolesContext } from "app"
import { SvgStarXxs } from "assets"
import { TEST_GAME_SRC } from "testConfig"
import { ModeratorPublicationContextMenu } from "ui/components/specific"
import { formatRating } from "utils"

import { PublicationCardProps } from "./types"

export const GamePublicationCard = memo(({ id, title, authorTitle, categoryTitle, rating }: PublicationCardProps) => {
  const { isModerator } = useSiteRolesContext()

  return (
    <div className="relative h-117.5 w-67.75 overflow-hidden rounded-lg bg-gray-100 hover:bg-gray-200" title={title}>
      <div className="h-91.25 overflow-hidden">
        <img src={TEST_GAME_SRC} className="size-full object-cover" />
      </div>
      <div className="flex flex-col gap-1 p-3">
        <span className="flex h-5 items-center text-base">
          {rating > 0 && (
            <>
              {formatRating(rating)} <SvgStarXxs className="fill-favorite" />
            </>
          )}
        </span>
        <span className="truncate text-sm font-medium leading-4.25 text-gray-800">{title}</span>
        <span className="truncate text-2xs leading-4 text-gray-500">{authorTitle}</span>
        <span className="truncate text-2xs leading-4 text-gray-500">{categoryTitle}</span>
      </div>

      {isModerator && (
        <ModeratorPublicationContextMenu
          publicationId={id}
          publicationTitle={title}
          className="absolute right-1 top-1"
        />
      )}
    </div>
  )
})
