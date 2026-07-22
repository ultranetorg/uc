import { memo } from "react"
import { Link } from "react-router-dom"
import { TFunction } from "i18next"

import { SvgSiteLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

export type LogoDropdownButtonProps = {
  t: TFunction
  storeId: string
  title: string
  imageFileId?: string
  publishersCount: number
}

export const LogoDropdownButton = memo(
  ({ t, storeId, title, imageFileId, publishersCount }: LogoDropdownButtonProps) => (
    <div className="flex cursor-pointer items-center rounded-xl p-1" title={title}>
      <div className="flex select-none items-center gap-3">
        <Link to={routes.store(storeId)} className="size-10 overflow-hidden rounded-lg">
          <ImageFallback src={buildFileUrl(imageFileId)} fallback={<SvgSiteLogo className="size-10" />} />
        </Link>
        <div className="flex flex-col">
          <Link to={routes.about(storeId)} className="w-44 truncate text-2base font-medium leading-5.25 text-gray-800">
            {title}
          </Link>
          <span className="text-2xs font-medium leading-5 text-gray-500">
            {publishersCount} {t("common:publishers", { count: publishersCount })}
          </span>
        </div>
      </div>
    </div>
  ),
)
