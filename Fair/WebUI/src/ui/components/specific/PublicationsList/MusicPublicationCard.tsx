import { TEST_MUSIC_SRC } from "testConstants"

import { PublicationCardProps } from "./types"

export const MusicPublicationCard = ({ title, authorTitle, categoryTitle }: PublicationCardProps) => (
  <div className="w-67.75 h-87.75 overflow-hidden rounded-lg bg-gray-100 hover:bg-gray-200" title={title}>
    <div className="h-67.5 overflow-hidden">
      <img src={TEST_MUSIC_SRC} className="h-full w-full object-cover" />
    </div>
    <div className="flex flex-col gap-1 p-3">
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-sm font-medium leading-4.25 text-gray-800">
        {title}
      </span>
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2xs leading-4 text-gray-500">
        {authorTitle}
      </span>
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2xs leading-4 text-gray-500">
        {categoryTitle}
      </span>
    </div>
  </div>
)
