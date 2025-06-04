import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { formatTitle } from "utils"

export type CategoryCardProps = {
  active?: boolean
  title: string
}

export const CategoryCard = memo(({ active, title }: CategoryCardProps) => (
  <div
    className={twMerge(
      "flex select-none items-center justify-center whitespace-nowrap rounded bg-gray-100 px-3 py-2 text-2sm leading-4.5 text-gray-800 hover:bg-gray-200",
      active && "pointer-events-none bg-gray-950 text-gray-0 hover:bg-gray-950",
    )}
    title={title}
  >
    {formatTitle(title)}
  </div>
))
