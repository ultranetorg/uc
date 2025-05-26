import { twMerge } from "tailwind-merge"

import { StarSvg } from "assets"
import { memo } from "react"

export type SiteCardProps = {
  title: string
  description?: string
  isStarred?: boolean
}

export const SiteCard = memo(({ title, description, isStarred = false }: SiteCardProps) => (
  <>
    <div
      className="w-55 group relative flex h-48 flex-col gap-4 rounded-lg bg-gray-100 px-2 py-6 hover:bg-gray-200"
      title={title}
    >
      <div className="h-18 w-18 mx-auto rounded-2xl bg-gray-700" />
      <div className="w-51 flex flex-col gap-2 text-center">
        <span className="leading-4.5 text-2sm overflow-hidden text-ellipsis whitespace-nowrap font-semibold text-gray-800">
          {title}
        </span>
        {description && (
          <span className="text-2xs line-clamp-2 h-8 overflow-hidden leading-4 text-gray-500">{description}</span>
        )}
      </div>
      <StarSvg
        className={twMerge(
          "invisible absolute right-3 top-3 group-hover:visible",
          isStarred !== true ? "stroke-gray-400" : "stroke-favorite fill-favorite",
        )}
      />
    </div>
  </>
))
