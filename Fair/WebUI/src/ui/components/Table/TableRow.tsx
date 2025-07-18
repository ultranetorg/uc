import { PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type TableRowBaseProps = {
  disableHover?: boolean
}

export type TableRowProps = PropsWithChildren & PropsWithClassName & TableRowBaseProps

export const TableRow = ({ children, className, disableHover }: TableRowProps) => (
  <div
    className={twMerge(
      "flex h-16 items-center justify-between gap-6 px-4 py-3",
      disableHover !== true && "cursor-pointer hover:bg-gray-300",
      className,
    )}
  >
    {children}
  </div>
)
