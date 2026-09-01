import { memo } from "react"
import { Link } from "react-router-dom"

import { SvgStoreLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

export interface FavoriteStoreItemProps {
  storeId: string
  name: string
  logoId?: string
}

export const FavoriteStoreItem = memo(({ storeId, name, logoId }: FavoriteStoreItemProps) => (
  <Link to={routes.store(storeId)} title={name}>
    <div className="size-8 overflow-hidden rounded">
      <ImageFallback className="size-8" src={buildFileUrl(logoId)} fallback={<SvgStoreLogo className="size-8" />} />
    </div>
  </Link>
))
