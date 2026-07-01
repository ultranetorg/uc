import { memo } from "react"
import { times } from "lodash"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { ChevronLeftSvg, SvgChevronRight } from "assets"

import { PaginationItem } from "./PaginationItem"

type PaginationBaseProps = {
  page: number
  pagesCount: number
  onPageChange(page: number): void
}

export type PaginationProps = PropsWithClassName & PaginationBaseProps

export const Pagination = memo(({ className, page, pagesCount, onPageChange }: PaginationProps) => {
  if (pagesCount <= 0) {
    return null
  }

  return (
    <div className={twMerge("flex gap-1", className)}>
      <PaginationItem disabled={page === 0} onClick={() => onPageChange(page - 1)}>
        <ChevronLeftSvg />
      </PaginationItem>
      {times(pagesCount, i => (
        <PaginationItem key={i} active={i === page} onClick={() => onPageChange(i)}>
          {i}
        </PaginationItem>
      ))}
      <PaginationItem disabled={page === pagesCount - 1} onClick={() => onPageChange(page + 1)}>
        <SvgChevronRight />
      </PaginationItem>
    </div>
  )
})
