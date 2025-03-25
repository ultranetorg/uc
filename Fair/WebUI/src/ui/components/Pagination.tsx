import { times } from "lodash"
import { twMerge } from "tailwind-merge"

export type PaginationProps = {
  page: number
  pagesCount: number
  onClick(page: number): void
}

export const Pagination = ({ page, pagesCount, onClick }: PaginationProps) => {
  return (
    <div className="flex gap-2">
      Pagination{" "}
      {times(pagesCount, i => (
        <div className={twMerge("cursor-pointer", i === page && "text-red-500")} key={i} onClick={() => onClick(i)}>
          {i}
        </div>
      ))}
    </div>
  )
}
