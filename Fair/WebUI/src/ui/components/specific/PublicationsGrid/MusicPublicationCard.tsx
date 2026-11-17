import { memo } from "react"

import { SvgFileEarmarkMusicSm, SvgStarXxs } from "assets"
import { PublicationCardContextThreeDotsMenu } from "ui/components/specific"
import { formatAverageRating } from "utils"

import { PublicationCardProps } from "./types"

export const MusicPublicationCard = memo(({ id, title, authorTitle, categoryTitle, rating }: PublicationCardProps) => {
  const formattedRating = formatAverageRating(rating)

  return (
    <div className="relative w-full overflow-hidden rounded-lg bg-gray-100 hover:bg-gray-200" title={title}>
      <div className="flex aspect-square items-center justify-center overflow-hidden bg-gray-400">
        <SvgFileEarmarkMusicSm className="size-20 stroke-gray-300" />
      </div>
      <div className="flex flex-col gap-1 p-3">
        <span className="flex h-5 items-center text-base" title={formattedRating}>
          {rating > 0 && (
            <>
              {formattedRating} <SvgStarXxs className="fill-favorite" />
            </>
          )}
        </span>
        <span className="truncate text-sm leading-4.25 font-medium">{title}</span>
        <span className="truncate text-2xs leading-4 text-gray-500">{authorTitle}</span>
        <span className="truncate text-2xs leading-4 text-gray-500">{categoryTitle}</span>
      </div>

      <PublicationCardContextThreeDotsMenu
        publicationId={id}
        publicationTitle={title}
        className="absolute top-1 right-1"
      />
    </div>
  )
})
