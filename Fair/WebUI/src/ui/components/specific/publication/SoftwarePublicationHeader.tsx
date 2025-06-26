import { SOFTWARE_HEADER_SRC } from "testConfig"
import { formatSoftwareCategories } from "utils"

export type SoftwarePublicationHeaderProps = {
  title: string
  categories: string[]
}

export const SoftwarePublicationHeader = ({ title, categories }: SoftwarePublicationHeaderProps) => (
  <div className="flex items-center gap-4">
    <div className="w-17 h-17 overflow-hidden rounded-full">
      <img src={SOFTWARE_HEADER_SRC} className="h-full w-full object-cover" />
    </div>
    <div className="flex flex-col gap-2 text-gray-800">
      <span className="text-3.5xl font-semibold leading-10">{title}</span>
      <span className="text-2xs font-medium leading-4">{formatSoftwareCategories(categories)}</span>
    </div>
  </div>
)
