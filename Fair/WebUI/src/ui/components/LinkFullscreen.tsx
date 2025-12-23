import { PropsWithChildren } from "react"
import { Link, To, useLocation, useParams } from "react-router-dom"

import { PropsWithClassName } from "types"

export type LinkFullscreenState = {
  backgroundLocation?: Location
  siteId?: string
}

type LinkFullscreenBaseProps = {
  location?: Location
  to: To
  params?: Record<string, unknown>
}

export type LinkFullscreenProps = PropsWithChildren & PropsWithClassName & LinkFullscreenBaseProps

export const LinkFullscreen = ({ children, className, location, to, params }: LinkFullscreenProps) => {
  const currentLocation = useLocation()
  const { siteId } = useParams()

  return (
    <Link
      className={className}
      to={to}
      state={{ backgroundLocation: location ?? currentLocation, siteId, ...params } as LinkFullscreenState}
    >
      {children}
    </Link>
  )
}
