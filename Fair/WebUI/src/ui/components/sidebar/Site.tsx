import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { StarSvg } from "assets"

export type SiteProps = {
  title: string
  isStarred?: boolean
}

export const Site = memo(({ title, isStarred }: SiteProps) => (
  <div className="group flex items-center gap-3">
    <div className="h-10 w-10 rounded-lg bg-gray-700" />
    <span className="w-36 flex-grow overflow-hidden text-ellipsis whitespace-nowrap text-2xs font-medium leading-4 text-gray-800 group-hover:font-semibold">
      {title}
    </span>
    <StarSvg
      className={twMerge(
        "invisible h-5 w-5 group-hover:visible",
        isStarred !== true ? "stroke-gray-400" : "fill-favorite stroke-favorite",
      )}
    />
  </div>
))
