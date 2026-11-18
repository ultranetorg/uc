import { memo, ReactNode } from "react"

import { SvgSoftwareLogo3Xl } from "assets/fallback"
import { ImageFallback, LinkFullscreen } from "ui/components"
import { buildFileUrl, routes } from "utils"

export type ModerationPublicationHeaderProps = {
  storeId: string
  title?: string
  logoId?: string
  authorId: string
  authorTitle: string
  components?: ReactNode
}

export const ModerationPublicationHeader = memo(
  ({ storeId, title, logoId, authorId, authorTitle, components }: ModerationPublicationHeaderProps) => (
    <div className="flex min-w-0 flex-wrap items-center gap-4 md:flex-nowrap md:justify-between">
      <div className="size-14 shrink-0 overflow-hidden rounded-2xl sm:size-17" title={title}>
        <ImageFallback src={buildFileUrl(logoId)} fallback={<SvgSoftwareLogo3Xl className="size-full" />} />
      </div>
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        {title && (
          <span className="truncate text-2xl leading-7.5 font-semibold" title={title}>
            {title}
          </span>
        )}
        <LinkFullscreen to={routes.publisher(storeId, authorId)} className="w-fit max-w-full" title={authorTitle}>
          <span className="block truncate text-2sm leading-5 font-medium">{authorTitle}</span>
        </LinkFullscreen>
      </div>
      {components && <div className="w-full md:w-auto md:shrink-0">{components}</div>}
    </div>
  ),
)
