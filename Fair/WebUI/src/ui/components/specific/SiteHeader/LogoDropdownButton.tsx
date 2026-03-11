import { memo } from "react"

import { SvgSiteLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl } from "utils"

export type LogoDropdownButtonProps = {
  title: string
  imageFileId?: string
}

export const LogoDropdownButton = memo(({ title, imageFileId }: LogoDropdownButtonProps) => (
  <div className="flex cursor-pointer items-center rounded-xl p-1" title={title}>
    <div className="flex select-none items-center gap-3">
      <div className="size-10 overflow-hidden rounded-lg">
        <ImageFallback src={buildFileUrl(imageFileId)} fallback={<SvgSiteLogo className="size-10" />} />
      </div>
      <span className="w-44 truncate text-2base font-medium leading-5.25 text-gray-800">{title}</span>
    </div>
  </div>
))
