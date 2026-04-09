import { PropsWithChildren } from "react"
import { Link, LinkProps, To, useLocation, useParams } from "react-router-dom"

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

export type LinkFullscreenProps = PropsWithChildren &
  PropsWithClassName &
  Pick<LinkProps, "title"> &
  LinkFullscreenBaseProps

export const LinkFullscreen = ({ children, className, title, location, to, params }: LinkFullscreenProps) => {
  const currentLocation = useLocation()
  const { siteId } = useParams()

  return (
    <Link
      className={className}
      to={to}
      state={{ backgroundLocation: location ?? currentLocation, siteId, ...params } as LinkFullscreenState}
      title={title}
    >
      {children}
    </Link>
  )
}
