import { memo } from "react"

import avatarPlaceholder from "assets/images/account-avatar-placeholder-9xl.png"
import { buildSrc } from "utils"

export type PublicationInfoProps = {
  avatar?: string
  categoryTitle?: string
  title: string
}

export const PublicationInfo = memo(({ avatar, categoryTitle, title }: PublicationInfoProps) => (
  <div className="flex items-center gap-2" title={title}>
    <div className="h-8 w-8 shrink-0 overflow-hidden rounded-full">
      <img className="h-full w-full object-cover" src={buildSrc(avatar, avatarPlaceholder)} />
    </div>
    <div className="flex flex-col overflow-hidden">
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-sm leading-4.25">{title}</span>
      {categoryTitle && (
        <span className="overflow-hidden text-ellipsis whitespace-nowrap text-xs leading-3.75 text-gray-500">
          {categoryTitle}
        </span>
      )}
    </div>
  </div>
))
