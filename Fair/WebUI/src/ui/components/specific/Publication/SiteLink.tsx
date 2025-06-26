import { SvgBoxArrowUpRight } from "assets"
import { Link, To } from "react-router-dom"

export type SiteLinkProps = {
  to: To
  label: string
}

export const SiteLink = ({ to, label }: SiteLinkProps) => (
  <Link
    to={to}
    className="flex items-center justify-between rounded-lg border border-gray-300 bg-gray-100 px-6 py-4 text-2sm font-medium leading-4.5"
    title={to.toString()}
  >
    {label}
    <SvgBoxArrowUpRight className="fill-gray-800" />
  </Link>
)
