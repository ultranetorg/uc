import { memo } from "react"

import { SvgSoftwareLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl } from "utils"

export type GridCardProps = {
  productTitle: string
  authorTitle: string
  avatarId?: string
}

export const GridCard = memo(({ productTitle, authorTitle, avatarId }: GridCardProps) => (
  <div
    className="flex h-48.5 w-55 flex-col gap-4 rounded-lg bg-gray-100 px-2 py-6 hover:bg-gray-200"
    title={productTitle}
  >
    <div className="mx-auto size-18 overflow-hidden rounded-2xl">
      <ImageFallback src={buildFileUrl(avatarId)} fallback={<SvgSoftwareLogo className="size-18" />} />
    </div>
    <div className="flex w-51 flex-col gap-2 text-center">
      <span className="truncate text-2sm font-semibold leading-4.5 text-gray-800">{productTitle}</span>
      <span className="line-clamp-2 h-8 overflow-hidden text-2xs leading-4 text-gray-500">{authorTitle}</span>
    </div>
  </div>
))
