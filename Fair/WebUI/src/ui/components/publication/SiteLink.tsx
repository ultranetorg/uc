import { memo } from "react"
import { Link, To } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { SvgBoxArrowUpRight } from "assets"
import { PropsWithClassName } from "types"

export type SiteLinkBaseProps = {
  to: To
  label: string
}

export type SiteLinkProps = PropsWithClassName & SiteLinkBaseProps

export const SiteLink = memo(({ className, to, label }: SiteLinkProps) => (
  <Link
    to={to}
    className={twMerge(
      "flex items-center justify-between rounded-lg border border-[#D7DDEB] bg-[#F3F5F8] px-6 py-4 text-2sm font-medium leading-4.5",
      className,
    )}
    title={to.toString()}
  >
    {label}
    <SvgBoxArrowUpRight className="fill-gray-800" />
  </Link>
))
