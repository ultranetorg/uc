import { SvgChevronLeft, SvgChevronRight } from "assets"
import { memo, useCallback } from "react"
import ReactPaginate from "react-paginate"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

// NOTE: css hack used for preventing clicking on break (...) label.
import "./Pagination.css"

type PaginationBaseProps = {
  page: number
  pageRangeDisplayed?: number
  pagesCount: number
  onPageChange?(page: number): void
}

export type PaginationProps = PropsWithClassName & PaginationBaseProps

export const Pagination = memo((props: PaginationProps) => {
  const { className, page, pageRangeDisplayed = 2, pagesCount, onPageChange } = props

  const handlePageChange = useCallback((e: any) => onPageChange && onPageChange(e.selected), [onPageChange])

  return (
    <ReactPaginate
      className={twMerge(className)}
      forcePage={page}
      pageCount={pagesCount}
      onPageChange={handlePageChange}
      previousLabel={<SvgChevronLeft className="m-auto block h-9" />}
      nextLabel={<SvgChevronRight className="m-auto block h-9" />}
      marginPagesDisplayed={1}
      pageRangeDisplayed={pageRangeDisplayed}
      containerClassName="flex select-none items-center gap-1"
      nextClassName="h-9 w-9 rounded-lg hover:bg-dark-alpha-100"
      nextLinkClassName="stroke-gray-200"
      previousClassName="h-9 w-9 rounded-lg hover:bg-dark-alpha-100"
      previousLinkClassName="stroke-gray-200"
      disabledClassName="hover:bg-transparent"
      disabledLinkClassName="cursor-default opacity-40"
      pageClassName="rounded-lg hover:bg-dark-alpha-100 text-gray-200"
      pageLinkClassName="block h-9 w-9 text-center leading-9"
      activeClassName="blocked relative hover:bg-transparent"
      activeLinkClassName="block h-9 w-9 text-cyan-500"
      breakClassName="blocked relative h-9 w-9 text-center text-gray-200"
      breakLinkClassName="cursor-default leading-9"
    />
  )
})
