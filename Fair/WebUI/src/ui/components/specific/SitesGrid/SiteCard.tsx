import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { StarSvg } from "assets"
import avatarFallback from "assets/fallback/site-logo-10xl.png"
import { buildFileUrl } from "utils"

export type SiteCardProps = {
  title: string
  description?: string
  imageFileId?: string
  isStarred?: boolean
}

export const SiteCard = memo(({ title, description, imageFileId, isStarred = false }: SiteCardProps) => (
  <>
    <div
      className="group relative flex h-48.5 w-55 flex-col gap-4 rounded-lg bg-gray-100 px-2 py-6 hover:bg-gray-200"
      title={title}
    >
      <div className="mx-auto size-18 overflow-hidden rounded-2xl">
        <img
          className="size-full object-cover object-center"
          src={imageFileId ? buildFileUrl(imageFileId) : avatarFallback}
          loading="lazy"
          onError={e => {
            e.currentTarget.onerror = null
            e.currentTarget.src = avatarFallback
          }}
        />
      </div>
      <div className="flex w-51 flex-col gap-2 text-center">
        <span className="truncate text-2sm font-semibold leading-4.5 text-gray-800">{title}</span>
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
