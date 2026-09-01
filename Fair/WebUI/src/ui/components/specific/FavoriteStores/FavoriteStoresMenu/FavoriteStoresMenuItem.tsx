import { memo } from "react"
import { Link } from "react-router-dom"

import { SvgStoreLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

export type FavoriteStoresMenuItemProps = {
  storeId: string
  title: string
  fileId?: string
  onClick: () => void
}

export const FavoriteStoresMenuItem = memo(({ storeId, title, fileId, onClick }: FavoriteStoresMenuItemProps) => (
  <Link to={routes.store(storeId)} title={title} onClick={onClick}>
    <div className="flex cursor-pointer items-center gap-3 rounded p-2 text-2sm leading-5 hover:bg-gray-700">
      <div className="size-10 shrink-0 overflow-hidden rounded">
        <ImageFallback className="size-10" src={buildFileUrl(fileId)} fallback={<SvgStoreLogo className="size-10" />} />
      </div>
      <span className="w-full overflow-hidden text-ellipsis text-nowrap font-medium text-white">{title}</span>
    </div>
  </Link>
))
