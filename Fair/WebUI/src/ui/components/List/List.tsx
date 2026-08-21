import { memo, PropsWithChildren, ReactNode } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ListBaseProps = { header?: ReactNode }

export type ListProps = PropsWithChildren & PropsWithClassName & ListBaseProps

export const List = memo(({ children, className, header }: ListProps) => (
  <div className={twMerge("overflow-hidden rounded border border-gray-300 bg-gray-100", className)}>
    {header}
    <div className="divide-y divide-gray-300">{children}</div>
  </div>
))
