import { memo } from "react"
import { times } from "lodash"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { ChevronLeftSvg, SvgChevronRight } from "assets"

import { PaginationItem } from "./PaginationItem"

type NextPaginationBaseProps = {
  hasNext: boolean
  page: number
  loadedPages: number
  onPageChange(page: number): void
}

export type NextPaginationProps = PropsWithClassName & NextPaginationBaseProps

export const NextPagination = memo(({ className, hasNext, page, loadedPages, onPageChange }: NextPaginationProps) => {
  if (loadedPages <= 0) {
    return null
  }

  return (
    <div className={twMerge("flex gap-4", className)}>
      <PaginationItem disabled={page === 0} onClick={() => onPageChange(page - 1)}>
        <ChevronLeftSvg />
      </PaginationItem>
      <div className="flex gap-1">
        {times(loadedPages, i => (
          <PaginationItem key={i} active={i === page} onClick={() => onPageChange(i)}>
            {i}
          </PaginationItem>
        ))}
      </div>
      <PaginationItem className="w-fit items-center pl-2.5" disabled={!hasNext} onClick={() => onPageChange(page + 1)}>
        Next
        <SvgChevronRight />
      </PaginationItem>
    </div>
  )
})
