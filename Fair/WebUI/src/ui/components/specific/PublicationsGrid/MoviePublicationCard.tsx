import { TEST_MOVIE_SRC } from "testConfig"

import { ModeratorPublicationContextMenu } from "ui/components/specific"

import { PublicationCardProps } from "./types"

export const MovePublicationCard = ({ id, title, authorTitle }: PublicationCardProps) => (
  <div className="relative h-106.5 w-67.75 overflow-hidden rounded-lg bg-gray-100 hover:bg-gray-200" title={title}>
    <div className="h-91.25 overflow-hidden">
      <img src={TEST_MOVIE_SRC} className="size-full object-cover" />
    </div>
    <div className="flex flex-col gap-1 p-3">
      <span className="truncate text-sm font-medium leading-4.25 text-gray-800">{title}</span>
      <span className="truncate text-2xs leading-4 text-gray-500">{authorTitle}</span>
    </div>

    <ModeratorPublicationContextMenu publicationId={id} className="absolute right-1 top-1" />
  </div>
)
