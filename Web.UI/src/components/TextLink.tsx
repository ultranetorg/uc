import { memo } from "react"
import { Link, To } from "react-router-dom"

export type TextLinkProps = {
  text: string
  to: To
}

export const TextLink = memo(({ text, to }: TextLinkProps) => (
  <Link
    className="overflow-hidden text-ellipsis whitespace-nowrap text-cyan-500 no-underline hover:no-underline"
    to={to}
  >
    {text}
  </Link>
))
