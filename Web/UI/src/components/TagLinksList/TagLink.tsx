import { memo } from "react"
import { Link } from "react-router-dom"

export type TagLinkProps = {
  label: string
  to: string
}

export const TagLink = memo(({ label, to }: TagLinkProps) => (
  <Link to={to} className="font-medium leading-[17px] text-cyan-500">
    {label}
  </Link>
))
