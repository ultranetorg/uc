import { memo } from "react"
import { Link } from "react-router-dom"

import { SvgStoreLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

export interface CurrentStoreProps {
  storeId: string
  logoFileId?: string
  title: string
}

export const CurrentStore = memo(({ storeId, logoFileId, title }: CurrentStoreProps) => {
  return (
    <Link to={routes.store(storeId)}>
      <div className="flex items-center gap-3 overflow-hidden rounded-lg py-1.5">
        <div className="shrink-0 overflow-hidden rounded-lg">
          <ImageFallback
            className="size-8"
            src={buildFileUrl(logoFileId)}
            fallback={<SvgStoreLogo className="size-8" />}
          />
        </div>
        <span className="left-4 min-w-0 truncate text-2xs font-medium">{title}</span>
      </div>
    </Link>
  )
})
