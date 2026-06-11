import { memo } from "react"
import { AuthorBaseAvatar } from "types"

import avatarFallback from "assets/fallback/author-8.png"
import { ImageFallback } from "ui/components"
import { buildFileUrl } from "utils"

export type PublisherInfoProps = Pick<AuthorBaseAvatar, "title" | "avatarId">

export const PublisherInfo = memo(({ title, avatarId }: PublisherInfoProps) => {
  return (
    <div className="flex items-center gap-2" title={title}>
      <div className="size-8 overflow-hidden rounded-full">
        <ImageFallback src={buildFileUrl(avatarId)} fallbackSrc={avatarFallback} />
      </div>
      <span className="select-none text-2sm font-medium leading-5">{title}</span>
    </div>
  )
})
