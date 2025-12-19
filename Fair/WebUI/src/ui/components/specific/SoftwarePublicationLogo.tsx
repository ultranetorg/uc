import { SvgSoftwareLogo } from "assets/fallback"
import { ImageFallback } from "ui/components"
import { buildFileUrl, formatSoftwareCategories } from "utils"

export type SoftwarePublicationLogoProps = {
  title: string
  showLogo?: boolean
  logoFileId?: string
  categories?: string[]
}

export const SoftwarePublicationLogo = ({
  title,
  showLogo = true,
  logoFileId,
  categories,
}: SoftwarePublicationLogoProps) => (
  <div className="flex items-center gap-4">
    {showLogo && (
      <div className="size-17 overflow-hidden rounded-2xl">
        <ImageFallback src={buildFileUrl(logoFileId)} fallback={<SvgSoftwareLogo className="size-17" />} />
      </div>
    )}
    <div className="flex flex-col gap-2 text-gray-800">
      <span className="text-3.5xl font-semibold leading-10">{title}</span>
      {categories && <span className="text-2xs font-medium leading-4">{formatSoftwareCategories(categories)}</span>}
    </div>
  </div>
)
