import { memo } from "react"
import { twMerge } from "tailwind-merge"

import avatarFallback from "assets/fallback/author-8.png"
import { ImageFallback } from "ui/components/ImageFallback"
import { buildFileUrl } from "utils"

export type AuthorImageTitleProps = {
  title: string
  authorFileId?: string
}

export const AuthorImageTitle = memo(({ title, authorFileId }: AuthorImageTitleProps) => (
  <div className="flex items-center gap-2">
    <div className="size-8 overflow-hidden rounded-full">
      <ImageFallback src={buildFileUrl(authorFileId)} fallbackSrc={avatarFallback} className="size-full object-cover" />
    </div>
    <span
      className={twMerge(
        "cursor-pointer overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800 hover:font-semibold",
      )}
    >
      {title}
    </span>
  </div>
))
