import { TEST_PUBLICATION_SMALL_SRC } from "testConstants"
import { PublicationExtended } from "types"

export type PublicationRowProps = Omit<PublicationExtended, "id" | "authorId" | "categoryId">

export const PublicationRow = ({ title, authorTitle, categoryTitle }: PublicationRowProps) => {
  return (
    <div
      className="flex cursor-pointer items-center gap-6 bg-gray-100 p-4 text-2sm leading-5 text-gray-900 hover:bg-gray-200"
      title={title}
    >
      <div className="flex w-1/2 max-w-[520px] items-center gap-2">
        <div className="h-8 w-8 flex-none overflow-hidden rounded-lg bg-gray-500">
          <img src={TEST_PUBLICATION_SMALL_SRC} className="h-full w-full object-cover" />
        </div>
        <span className="overflow-hidden text-ellipsis whitespace-nowrap">{title}</span>
      </div>
      <span className="w-1/4 max-w-60 overflow-hidden text-ellipsis whitespace-nowrap">{authorTitle}</span>
      <span className="w-1/4 max-w-60 overflow-hidden text-ellipsis whitespace-nowrap">{categoryTitle}</span>
    </div>
  )
}
