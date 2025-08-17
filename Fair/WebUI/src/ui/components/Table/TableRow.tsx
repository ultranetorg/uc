import { MouseEvent, PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type TableRowBaseProps = {
  onClick?: (e: MouseEvent<HTMLTableRowElement>) => void
}

export type TableRowProps = PropsWithChildren & PropsWithClassName & TableRowBaseProps

export const TableRow = ({ children, className, onClick }: TableRowProps) => (
  <tr
    className={twMerge("h-16 border-b border-b-gray-300", onClick && "cursor-pointer hover:bg-gray-300", className)}
    onClick={onClick}
  >
    {children}
  </tr>
)
