import { memo } from "react"

import { SvgSoftwareLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl } from "utils"

export type PublicationInfoProps = {
  avatarId?: string
  categoryTitle?: string
  title: string
}

export const PublicationInfo = memo(({ avatarId, categoryTitle, title }: PublicationInfoProps) => (
  <div className="flex items-center gap-2" title={title}>
    <div className="size-8 shrink-0 overflow-hidden rounded-lg">
      <ImageFallback src={buildFileUrl(avatarId)} fallback={<SvgSoftwareLogo className="size-8" />} />
    </div>
    <div className="flex flex-col overflow-hidden">
      <span className="truncate text-sm leading-4.25">{title}</span>
      {categoryTitle && <span className="truncate text-xs leading-3.75 text-gray-500">{categoryTitle}</span>}
    </div>
  </div>
))
