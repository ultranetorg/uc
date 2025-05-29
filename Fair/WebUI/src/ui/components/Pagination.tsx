import { memo, PropsWithChildren } from "react"
import { times } from "lodash"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { ChevronLeftSvg, ChevronRightSvg } from "assets"

type PaginationItemBaseProps = {
  active?: boolean
  disabled?: boolean
  onClick?: () => void
}

type PaginationItemProps = PropsWithChildren & PaginationItemBaseProps

const PaginationItem = ({ children, active, disabled, onClick }: PaginationItemProps) => (
  <div
    className={twMerge(
      "flex h-9 w-9 cursor-pointer select-none items-center justify-center rounded stroke-gray-950 font-bold leading-4.25 text-gray-900 hover:bg-dark-100/10",
      active === true && "bg-gray-950 text-zinc-100 hover:bg-gray-950 hover:text-zinc-100",
      disabled === true && "opacity-40 hover:bg-transparent",
    )}
    onClick={disabled ? undefined : onClick}
  >
    {children}
  </div>
)

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
        <ChevronRightSvg />
      </PaginationItem>
    </div>
  )
})
