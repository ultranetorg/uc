import { TEST_SOFTWARE_SRC } from "testConfig"
import { buildSrc } from "utils"

import { PublicationCardProps } from "./types"

export const SoftwarePublicationCard = ({ title, logo, authorTitle, categoryTitle }: PublicationCardProps) => (
  <div
    className="flex w-67.75 flex-col items-center justify-center gap-4 rounded-lg bg-gray-100 p-4 hover:bg-gray-200"
    title={title}
  >
    <div className="h-14 w-14 overflow-hidden rounded-lg">
      <img src={buildSrc(logo, TEST_SOFTWARE_SRC)} className="h-full w-full object-cover" />
    </div>
    <div className="flex w-40 flex-col gap-1 text-center">
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-4.5 text-gray-800">
        {title}
      </span>
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2xs leading-3.75 text-gray-500">
        {authorTitle}
      </span>
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2xs leading-3.75 text-gray-500">
        {categoryTitle}
      </span>
    </div>
  </div>
)
