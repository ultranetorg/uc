import { memo } from "react"

import avatarPlaceholder from "assets/fallback/software-logo-xl.png"
import { buildSrc } from "utils"

export type PublicationInfoProps = {
  avatar?: string
  categoryTitle?: string
  title: string
}

export const PublicationInfo = memo(({ avatar, categoryTitle, title }: PublicationInfoProps) => (
  <div className="flex items-center gap-2" title={title}>
    <div className="size-8 shrink-0 overflow-hidden rounded-lg">
      <img className="size-full object-cover object-center" src={buildSrc(avatar, avatarPlaceholder)} />
    </div>
    <div className="flex flex-col overflow-hidden">
      <span className="truncate text-sm leading-4.25">{title}</span>
      {categoryTitle && <span className="truncate text-xs leading-3.75 text-gray-500">{categoryTitle}</span>}
    </div>
  </div>
))
