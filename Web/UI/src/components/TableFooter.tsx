import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { Pagination } from "components"
import { PropsWithClassName } from "types"

type TableFooterBaseProps = {
  page: number
  pageSize: number
  totalItems: number
  labelDisplayedItems?: (from: number, to: number, totalCount: number) => string
  onPageChange(page: number): void
}

export type TableFooterProps = PropsWithClassName & TableFooterBaseProps

export const TableFooter = memo((props: TableFooterProps) => {
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
    <div className={twMerge("flex items-center justify-between", className)}>
      <span className="text-gray-200">{displayedItems}</span>
      <Pagination page={page} pagesCount={Math.ceil(totalItems / pageSize)} onPageChange={onPageChange} />
    </div>
  )
})
