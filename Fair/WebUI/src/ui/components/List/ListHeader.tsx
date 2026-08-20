import { memo, PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type ListHeaderProps = PropsWithChildren & PropsWithClassName

export const ListHeader = memo(({ children, className }: ListHeaderProps) => (
  <div
    className={twMerge(
      "flex items-center gap-6 border-b border-gray-300 bg-gray-100 px-2 py-1.5 text-gray-500",
      className,
    )}
  >
    {children}
  </div>
))
