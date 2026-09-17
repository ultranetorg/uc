import { memo } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { SvgStoreLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

export interface FavoriteStoreItemProps {
  storeId: string
  name: string
  logoId?: string
  isPending?: boolean
}

export const FavoriteStoreItem = memo(({ storeId, name, logoId, isPending }: FavoriteStoreItemProps) => (
  <Link
    to={routes.store(storeId)}
    title={name}
    className={twMerge(isPending && "pointer-events-none")}
    aria-disabled={isPending}
  >
    <div
      className={twMerge(
        "box-border size-8 overflow-hidden rounded hover:border hover:border-gray-500",
        isPending && "animate-pulse hover:border-transparent",
      )}
    >
      <ImageFallback className="size-8" src={buildFileUrl(logoId)} fallback={<SvgStoreLogo className="size-8" />} />
    </div>
  </Link>
))
