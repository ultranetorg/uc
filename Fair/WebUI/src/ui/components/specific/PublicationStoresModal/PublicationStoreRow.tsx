import { SvgChevronRight } from "assets"
import { PUBLICATIONS_STORE_ROW_TEST_IMAGE } from "testConfig"

export type PublicationStoreRowProps = {
  title: string
  publicationDate: number
}

export const PublicationStoreRow = ({ title, publicationDate }: PublicationStoreRowProps) => (
  <div className="flex cursor-pointer items-center gap-3 p-2 text-2sm leading-5">
    <div className="size-10 overflow-hidden rounded-lg">
      <img src={PUBLICATIONS_STORE_ROW_TEST_IMAGE} className="size-full object-cover" />
    </div>
    <span className="w-[45%] overflow-hidden text-ellipsis text-nowrap font-medium">{title}</span>
    <span className="w-[30%]">{publicationDate}</span>
    <SvgChevronRight className="w-[6%] stroke-gray-800" />
  </div>
)
