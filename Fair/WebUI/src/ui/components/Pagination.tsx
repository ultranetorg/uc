import { times } from "lodash"
import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type PaginationBaseProps = {
  page: number
  pagesCount: number
  onClick(page: number): void
}

export type PaginationProps = PropsWithClassName & PaginationBaseProps

export const Pagination = memo(({ className, page, pagesCount, onClick }: PaginationProps) => {
  return (
    <div className="flex gap-2">
      Pagination{" "}
      {times(pagesCount, i => (
        <div
          className={twMerge("cursor-pointer", i === page && "text-red-500", className)}
          key={i}
          onClick={() => onClick(i)}
        >
          {i}
        </div>
      ))}
    </div>
  )
})

type PaginationBaseProps2 = {
  page: number
  pagesCount: number
  hasNextPage: boolean
  onClick(page: number): void
  nextButtonLabel: string
}

export type PaginationProps2 = PropsWithClassName & PaginationBaseProps2

export const Pagination2 = memo(
  ({ className, page, pagesCount, hasNextPage, nextButtonLabel, onClick }: PaginationProps2) => {
    return (
      <div className="flex gap-2">
        {times(pagesCount, i => (
          <div
            className={twMerge("cursor-pointer", i === page && "text-red-500", className)}
            key={i}
            onClick={() => onClick(i)}
          >
            {i}
          </div>
        ))}
        {hasNextPage && <div onClick={() => onClick(page + 1)}>{nextButtonLabel}</div>}
      </div>
    )
  },
)
