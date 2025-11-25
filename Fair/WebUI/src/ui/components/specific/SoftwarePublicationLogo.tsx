import logoFallback from "assets/fallback/software-logo-9xl.png"
import { buildFileUrl, formatSoftwareCategories } from "utils"

export type SoftwarePublicationLogoProps = {
  title: string
  logoFileId?: string
  categories?: string[]
}

export const SoftwarePublicationLogo = ({ title, logoFileId, categories }: SoftwarePublicationLogoProps) => (
  <div className="flex items-center gap-4">
    <div className="size-17 overflow-hidden rounded-2xl">
      <img
        src={logoFileId ? buildFileUrl(logoFileId) : logoFallback}
        className="size-full object-cover object-center"
        onError={e => {
          e.currentTarget.onerror = null
          e.currentTarget.src = logoFallback
        }}
      />
    </div>
    <div className="flex flex-col gap-2 text-gray-800">
      <span className="text-3.5xl font-semibold leading-10">{title}</span>
      {categories && <span className="text-2xs font-medium leading-4">{formatSoftwareCategories(categories)}</span>}
    </div>
  </div>
)
