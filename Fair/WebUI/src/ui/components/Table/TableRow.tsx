import { PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type TableRowProps = PropsWithChildren & PropsWithClassName

export const TableRow = ({ children, className }: TableRowProps) => (
  <div
    className={twMerge(
      "flex h-16 cursor-pointer items-center justify-between gap-6 px-4 py-3 hover:bg-gray-300",
      className,
    )}
  >
    {children}
  </div>
)
