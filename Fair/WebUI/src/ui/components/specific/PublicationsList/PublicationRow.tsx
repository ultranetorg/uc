import { TEST_PUBLICATION_SMALL_SRC } from "testConfig"
import { Publication, PublicationExtended } from "types"
import { buildSrc } from "utils"
import { ModeratorPublicationMenu } from "../ModeratorPublicationMenu"

export type PublicationRowProps = Publication & Partial<Pick<PublicationExtended, "authorTitle">>

export const PublicationRow = ({ id, title, logo, authorTitle, categoryTitle }: PublicationRowProps) => {
  return (
    <div
      className="flex cursor-pointer items-center justify-between gap-6 bg-gray-100 p-4 text-2sm leading-5 text-gray-900 hover:bg-gray-200"
      title={title}
    >
      <div className="flex w-1/2 max-w-[520px] items-center gap-2">
        <div className="size-8 flex-none overflow-hidden rounded-lg bg-gray-500">
          <img src={buildSrc(logo, TEST_PUBLICATION_SMALL_SRC)} className="size-full object-cover" />
        </div>
        <span className="truncate font-medium">{title}</span>
      </div>
      {authorTitle && <span className="w-1/4 max-w-60 truncate">{authorTitle}</span>}
      <span className="w-1/4 max-w-60 truncate">{categoryTitle}</span>

      <ModeratorPublicationMenu publicationId={id} />
    </div>
  )
}
