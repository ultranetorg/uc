import { useCallback } from "react"
import ReactPaginate from "react-paginate"

import { PropsWithClassName } from "types"

type StylizedPaginationBaseProps = {
  page: number
  pagesCount: number
  onPageChange?(page: number): void
}

export type StylizedPaginationProps = StylizedPaginationBaseProps & PropsWithClassName

export const StylizedPagination = (props: StylizedPaginationProps) => {
  const { className, page, pagesCount, onPageChange } = props

  const handlePageChange = useCallback((e: any) => onPageChange && onPageChange(e.selected), [onPageChange])

  return (
    <ReactPaginate
      className={className}
      forcePage={page}
      pageCount={pagesCount}
      onPageChange={handlePageChange}
      previousLabel={"<"}
      nextLabel={">"}
      marginPagesDisplayed={1}
      pageRangeDisplayed={2}
      containerClassName="flex space-between gap-[14px] select-none"
      activeLinkClassName="text-[#3DC1F2]"
      nextLinkClassName="text-[#3DC1F2] ml-3"
      previousLinkClassName="text-[#3DC1F2] mr-3"
      disabledLinkClassName="text-[#444F53] hover:no-underline cursor-default"
    />
  )
}
