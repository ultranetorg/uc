import { memo } from "react"

import { SvgSoftwareLogo3Xl } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, formatSoftwareCategories } from "utils"

export type SoftwarePublicationLogoProps = {
  title?: string
  showLogo?: boolean
  logoFileId?: string
  categories?: string[]
}

export const SoftwarePublicationLogo = memo(
  ({ title, showLogo = true, logoFileId, categories }: SoftwarePublicationLogoProps) => (
    <div className="flex min-w-0 items-center gap-3 sm:gap-4">
      {showLogo && (
        <div className="size-14 shrink-0 overflow-hidden rounded-2xl sm:size-17">
          <ImageFallback src={buildFileUrl(logoFileId)} fallback={<SvgSoftwareLogo3Xl className="size-full" />} />
        </div>
      )}
      <div className="flex min-w-0 flex-col gap-2">
        {title && (
          <span className="text-2xl leading-8 font-semibold wrap-break-word sm:text-3.5xl sm:leading-10">{title}</span>
        )}
        {categories && <span className="text-2xs leading-4 font-medium">{formatSoftwareCategories(categories)}</span>}
      </div>
    </div>
  ),
)
