import { SOFTWARE_HEADER_SRC } from "testConfig"
import { ModeratorOptionsMenu } from "ui/components/specific"
import { buildSrc, formatSoftwareCategories } from "utils"

export type SoftwarePublicationHeaderProps = {
  id: string
  title: string
  logo?: string
  categories: string[]
}

export const SoftwarePublicationHeader = ({ id, title, logo, categories }: SoftwarePublicationHeaderProps) => (
  <div className="flex items-center gap-4">
    <div className="size-17 overflow-hidden rounded-2xl">
      <img src={buildSrc(logo, SOFTWARE_HEADER_SRC)} className="size-full object-cover" />
    </div>
    <div className="flex flex-col gap-2 text-gray-800">
      <span className="text-3.5xl font-semibold leading-10">{title}</span>
      <span className="text-2xs font-medium leading-4">{formatSoftwareCategories(categories)}</span>
    </div>
    <ModeratorOptionsMenu className="ml-auto" publicationId={id} />
  </div>
)
