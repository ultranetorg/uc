import { memo } from "react"
import { Link } from "react-router-dom"

import { SvgSiteLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl } from "utils"

export type LogoDropdownButtonProps = {
  siteId: string
  title: string
  imageFileId?: string
}

export const LogoDropdownButton = memo(({ siteId, title, imageFileId }: LogoDropdownButtonProps) => (
  <div className="flex cursor-pointer items-center rounded-xl p-1" title={title}>
    <div className="flex select-none items-center gap-3">
      <Link to={`/${siteId}`} className="size-10 overflow-hidden rounded-lg">
        <ImageFallback src={buildFileUrl(imageFileId)} fallback={<SvgSiteLogo className="size-10" />} />
      </Link>
      <Link to={`/${siteId}/i`} className="w-44 truncate text-2base font-medium leading-5.25 text-gray-800">
        {title}
      </Link>
    </div>
  </div>
))
