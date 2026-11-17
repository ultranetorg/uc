import { memo } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { SvgStoreLogo4Xl } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"
import { PropsWithClassName } from "types"

export interface FavoriteStoreItemBaseProps {
  storeId: string
  name: string
  logoId?: string
  isPending?: boolean
}

export type FavoriteStoreItemProps = PropsWithClassName & FavoriteStoreItemBaseProps

export const FavoriteStoreItem = memo(({ className, storeId, name, logoId, isPending }: FavoriteStoreItemProps) => (
  <Link
    to={routes.store(storeId)}
    title={name}
    className={twMerge(className, isPending && "pointer-events-none")}
    aria-disabled={isPending}
  >
    <div
      className={twMerge(
        "box-border size-8 overflow-hidden rounded-sm hover:border hover:border-gray-500",
        isPending && "animate-pulse hover:border-transparent",
      )}
    >
      <ImageFallback className="size-8" src={buildFileUrl(logoId)} fallback={<SvgStoreLogo4Xl className="size-8" />} />
    </div>
  </Link>
))
