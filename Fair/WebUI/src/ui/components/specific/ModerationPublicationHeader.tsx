import { memo, ReactNode } from "react"
import { useParams } from "react-router-dom"

import { SvgSoftwareLogo } from "assets/fallback"
import { ImageFallback, LinkFullscreen } from "ui/components"
import { buildFileUrl } from "utils"

export type ModerationPublicationHeaderProps = {
  title?: string
  logoId?: string
  authorId: string
  authorTitle: string
  components?: ReactNode
}

export const ModerationPublicationHeader = memo(
  ({ title, logoId, authorId, authorTitle, components }: ModerationPublicationHeaderProps) => {
    const { siteId } = useParams()

    return (
      <div className="flex min-w-0 items-center justify-between gap-4">
        <div className="size-17 overflow-hidden rounded-2xl" title={title}>
          <ImageFallback src={buildFileUrl(logoId)} fallback={<SvgSoftwareLogo className="size-17" />} />
        </div>
        <div className="flex flex-1 flex-col gap-1">
          {title && (
            <span className="truncate text-2xl font-semibold leading-7.5" title={title}>
              {title}
            </span>
          )}
          <LinkFullscreen to={`/${siteId}/a/${authorId}`} className="w-fit" title={authorTitle}>
            <span className="truncate text-2sm font-medium leading-5">{authorTitle}</span>
          </LinkFullscreen>
        </div>
        {components}
      </div>
    )
  },
)
