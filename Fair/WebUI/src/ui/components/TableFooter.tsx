import { useMemo } from "react"

import { PropsWithClassName } from "types"
import { twMerge } from "tailwind-merge"

import { StylizedPagination } from "./StylizedPagination"

type TableFooterBaseProps = {
  page: number
  pageSize: number
  totalItems: number
  labelDisplayedItems?: (from: number, to: number, totalCount: number) => string
  onPageChange(page: number): void
}

export type TableFooterProps = TableFooterBaseProps & PropsWithClassName

export const TableFooter = (props: TableFooterProps) => {
  const {
    className,
    page,
    pageSize,
    totalItems,
    labelDisplayedItems = (from, to, count) => `Showing from ${from} to ${to} of ${count} items`,
    onPageChange,
  } = props

  const displayedItems = useMemo(
    () => labelDisplayedItems(page * pageSize, Math.min(page * pageSize + pageSize, totalItems), totalItems),
    [labelDisplayedItems, page, pageSize, totalItems],
  )

  return (
    <div className={twMerge("flex justify-between", className)}>
      {displayedItems}
      <StylizedPagination page={page} pagesCount={Math.ceil(totalItems / pageSize)} onPageChange={onPageChange} />
    </div>
  )
}
