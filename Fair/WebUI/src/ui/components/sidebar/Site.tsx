import { memo, MouseEvent } from "react"
import { twMerge } from "tailwind-merge"

import { StarSvg } from "assets"
import avatarFallback from "assets/fallback/site-logo-3xl.png"
import { buildFileUrl } from "utils"

export type SiteProps = {
  disabled?: boolean
  disabledFavorite?: boolean
  title: string
  imageFileId?: string
  isStarred?: boolean
  onFavoriteClick: () => void
}

export const Site = memo(
  ({ disabled, disabledFavorite, title, imageFileId, isStarred, onFavoriteClick }: SiteProps) => {
    const handleFavoriteClick = (e: MouseEvent<SVGSVGElement>) => {
      e.preventDefault()

      if (!disabled) {
        onFavoriteClick()
      }
    }

    return (
      <div className={twMerge("group flex items-center gap-3", disabled && "opacity-60")}>
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
        <span
          className={twMerge(
            "w-36 grow truncate text-2xs font-medium leading-4 text-gray-800",
            !disabled && "group-hover:font-semibold",
          )}
        >
          {title}
        </span>
        {disabled ||
          (disabledFavorite !== true && (
            <StarSvg
              className={twMerge(
                "invisible size-5 group-hover:visible",
                isStarred !== true
                  ? "stroke-gray-400 hover:fill-favorite hover:stroke-favorite"
                  : "fill-favorite stroke-favorite hover:fill-transparent hover:stroke-gray-400",
              )}
              onClick={handleFavoriteClick}
            />
          ))}
      </div>
    )
  },
)
