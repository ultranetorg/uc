import { memo } from "react"

import { SvgChevronRight } from "assets"
import storeFallback from "assets/fallback/store-fallback-8.png"
import { ImageFallback } from "ui/components/ImageFallback"
import { buildFileUrl } from "utils"

export type PublicationStoreRowProps = {
  avatarId?: string
  title: string
  //publicationDate: number
}

export const PublicationStoreRow = memo(({ avatarId, title }: PublicationStoreRowProps) => (
  <div className="flex cursor-pointer items-center gap-3 p-2 text-2sm leading-5">
    <div className="size-10 shrink-0 overflow-hidden rounded-lg">
      <ImageFallback src={buildFileUrl(avatarId)} fallbackSrc={storeFallback} />
    </div>
    <span className="w-full overflow-hidden text-ellipsis text-nowrap font-medium">{title}</span>
    {/* <span className="w-[30%]">{publicationDate}</span> */}
    <SvgChevronRight className="w-[6%] stroke-gray-800" />
  </div>
))
