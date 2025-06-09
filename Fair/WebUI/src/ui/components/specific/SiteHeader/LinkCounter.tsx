import { memo, PropsWithChildren } from "react"
import { Link, To } from "react-router-dom"
import { twMerge } from "tailwind-merge"
import { PropsWithClassName } from "types"

type LinkCounterBaseProps = {
  count?: number
  to: To
}

export type LinkCounterProps = PropsWithClassName & PropsWithChildren & LinkCounterBaseProps

export const LinkCounter = memo(({ className, children, count, to }: LinkCounterProps) => (
  <Link to={to} className={twMerge("group flex gap-1 text-2sm leading-6", className)}>
    <span className="font-medium text-gray-800 group-hover:font-semibold">{children}</span>
    {count !== undefined && <span className="text-gray-500">({count})</span>}
  </Link>
))
