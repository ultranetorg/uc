import { TEST_BOOK_SRC } from "testConfig"

import { PublicationCardProps } from "./types"

export const BookPublicationCard = ({ title, categoryTitle }: PublicationCardProps) => (
  <div className="h-106.5 w-67.75 overflow-hidden rounded-lg bg-gray-100 hover:bg-gray-200" title={title}>
    <div className="h-91.25 overflow-hidden">
      <img src={TEST_BOOK_SRC} className="h-full w-full object-cover" />
    </div>
    <div className="flex flex-col gap-1 p-3">
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-sm font-medium leading-4.25 text-gray-800">
        {title}
      </span>
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2xs leading-4 text-gray-500">
        {categoryTitle}
      </span>
    </div>
  </div>
)
