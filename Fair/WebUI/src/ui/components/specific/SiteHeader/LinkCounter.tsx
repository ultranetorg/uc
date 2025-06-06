import { memo, PropsWithChildren } from "react"
import { Link, To } from "react-router-dom"

type LinkCounterBaseProps = {
  count?: number
  to: To
}

export type LinkCounterProps = PropsWithChildren & LinkCounterBaseProps

export const LinkCounter = memo(({ children, count, to }: LinkCounterProps) => (
  <Link to={to} className="group flex gap-1 text-2sm leading-4.5">
    <span className="font-medium text-gray-800 group-hover:font-semibold">{children}</span>
    {count && <span className="text-gray-500">({count})</span>}
  </Link>
))
