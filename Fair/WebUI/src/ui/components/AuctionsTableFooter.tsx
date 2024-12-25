import { useCallback, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { StylizedPagination } from "./StylizedPagination"
import { StylizedSelect } from "./StylizedSelect"

const itemsPerPageStyles = {
  controlStyle: {
    width: "80px",
  },
}

type AuctionsTableFooterBaseProps = {
  page: number
  pageSize: number
  totalItems: number
  itemsPerPage: number
  itemsPerPageValues: number[]
  labelDisplayedItems?: (from: number, to: number, totalCount: number) => string
  onPageChange(value: number): void
  onItemsPerPageChange(value: number): void
}

type AuctionsTableFooterProps = AuctionsTableFooterBaseProps & PropsWithClassName

export const AuctionsTableFooter = (props: AuctionsTableFooterProps) => {
  const {
    className,
    page,
    pageSize,
    totalItems,
    itemsPerPage,
    itemsPerPageValues,
    labelDisplayedItems = (from, to, count) => `Showing from ${from} to ${to} of ${count} items`,
    onPageChange,
    onItemsPerPageChange,
  } = props

  const displayedItems = useMemo(
    () => labelDisplayedItems(page * pageSize, Math.min(page * pageSize + pageSize, totalItems), totalItems),
    [labelDisplayedItems, page, pageSize, totalItems],
  )

  const options = useMemo(
    () => itemsPerPageValues.map(value => ({ label: value.toString(), value: value })),
    [itemsPerPageValues],
  )

  const handleItemsPerPageChange = useCallback(
    (value: any) => onItemsPerPageChange && onItemsPerPageChange(value.value),
    [onItemsPerPageChange],
  )

  return (
    <div
      className={twMerge(
        "flex flex-col items-center justify-center gap-0 sm:gap-3 md:flex-row md:justify-between md:gap-6",
        className,
      )}
    >
      <div className="flex items-center gap-3">
        View
        <StylizedSelect
          isSearchable={false}
          options={options}
          onChange={handleItemsPerPageChange}
          defaultValue={itemsPerPageValues[0]}
          value={itemsPerPage}
          {...itemsPerPageStyles}
        />
        items per page
      </div>
      {displayedItems}
      <StylizedPagination page={page} pagesCount={Math.ceil(totalItems / pageSize)} onPageChange={onPageChange} />
    </div>
  )
}
