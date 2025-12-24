import { memo } from "react"

import softwareLogo from "assets/fallback/software-logo-xl.png"
import { buildFileUrl } from "utils"

export type PublicationInfoProps = {
  avatarId?: string
  categoryTitle?: string
  title: string
}

export const PublicationInfo = memo(({ avatarId, categoryTitle, title }: PublicationInfoProps) => (
  <div className="flex items-center gap-2" title={title}>
    <div className="size-8 shrink-0 overflow-hidden rounded-lg">
      <img
        className="size-full object-cover object-center"
        loading="lazy"
        onError={e => {
          e.currentTarget.onerror = null
          e.currentTarget.src = softwareLogo
        }}
        src={buildFileUrl(avatarId!)}
      />
    </div>
    <div className="flex flex-col overflow-hidden">
      <span className="truncate text-sm leading-4.25">{title}</span>
      {categoryTitle && <span className="truncate text-xs leading-3.75 text-gray-500">{categoryTitle}</span>}
    </div>
  </div>
))
