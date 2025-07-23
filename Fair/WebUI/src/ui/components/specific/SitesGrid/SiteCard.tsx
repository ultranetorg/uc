import { twMerge } from "tailwind-merge"

import { StarSvg } from "assets"
import { memo } from "react"
import { buildSrc } from "utils"

export type SiteCardProps = {
  title: string
  description?: string
  avatar?: string
  isStarred?: boolean
}

export const SiteCard = memo(({ title, description, avatar, isStarred = false }: SiteCardProps) => (
  <>
    <div
      className="group relative flex h-48 w-55 flex-col gap-4 rounded-lg bg-gray-100 px-2 py-6 hover:bg-gray-200"
      title={title}
    >
      <div className="mx-auto h-18 w-18 overflow-hidden rounded-2xl">
        {avatar ? (
          <img className="h-full w-full object-cover" src={buildSrc(avatar)} />
        ) : (
          <div className="h-full w-full bg-gray-700" />
        )}
      </div>
      <div className="flex w-51 flex-col gap-2 text-center">
        <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-semibold leading-4.5 text-gray-800">
          {title}
        </span>
        {description && (
          <span className="line-clamp-2 h-8 overflow-hidden text-2xs leading-4 text-gray-500">{description}</span>
        )}
      </div>
      <StarSvg
        className={twMerge(
          "invisible absolute right-3 top-3 group-hover:visible",
          isStarred !== true ? "stroke-gray-400" : "fill-favorite stroke-favorite",
        )}
      />
    </div>
  </>
))
