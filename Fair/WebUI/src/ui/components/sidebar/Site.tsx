import { memo, MouseEvent } from "react"
import { twMerge } from "tailwind-merge"

import { StarSvg } from "assets"
import avatarFallback from "assets/fallback/site-logo-3xl.png"
import { buildFileUrl } from "utils"

export type SiteProps = {
  title: string
  imageFileId?: string
  isStarred?: boolean
  onFavoriteClick: () => void
}

export const Site = memo(({ title, imageFileId, isStarred, onFavoriteClick }: SiteProps) => {
  const handleFavoriteClick = (e: MouseEvent<SVGSVGElement>) => {
    e.preventDefault()
    onFavoriteClick()
  }

  return (
    <div className="group flex items-center gap-3">
      <div className="size-10 overflow-hidden rounded-lg bg-gray-700">
        <img
          src={imageFileId ? buildFileUrl(imageFileId) : avatarFallback}
          alt="Logo"
          className="size-full object-contain object-center"
          loading="lazy"
          onError={e => {
            e.currentTarget.onerror = null
            e.currentTarget.src = avatarFallback
          }}
        />
      </div>
      <span className="w-36 grow truncate text-2xs font-medium leading-4 text-gray-800 group-hover:font-semibold">
        {title}
      </span>
      <StarSvg
        className={twMerge(
          "invisible h-5 w-5 group-hover:visible",
          isStarred !== true ? "stroke-gray-400" : "fill-favorite stroke-favorite",
        )}
        onClick={handleFavoriteClick}
      />
    </div>
  )
})
