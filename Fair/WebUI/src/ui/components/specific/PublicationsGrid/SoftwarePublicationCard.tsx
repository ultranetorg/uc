import { TEST_SOFTWARE_SRC } from "testConfig"
import { buildSrc } from "utils"

import { ModeratorPublicationMenu } from "ui/components/specific"
import { PublicationCardProps } from "./types"

export const SoftwarePublicationCard = ({ id, title, logo, authorTitle, categoryTitle }: PublicationCardProps) => (
  <div
    className="relative flex w-67.75 flex-col items-center justify-center gap-4 rounded-lg bg-gray-100 p-4 hover:bg-gray-200"
    title={title}
  >
    <div className="size-14 overflow-hidden rounded-lg">
      <img src={buildSrc(logo, TEST_SOFTWARE_SRC)} className="size-full object-cover" />
    </div>
    <div className="flex w-40 flex-col gap-1 text-center">
      <span className="truncate text-2sm font-medium leading-4.5 text-gray-800">{title}</span>
      <span className="truncate text-2xs leading-3.75 text-gray-500">{authorTitle}</span>
      <span className="truncate text-2xs leading-3.75 text-gray-500">{categoryTitle}</span>
    </div>

    <ModeratorPublicationMenu publicationId={id} className="absolute right-1 top-1" />
  </div>
)
