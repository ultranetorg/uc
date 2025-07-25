import { PropsWithChildren } from "react"
import { Link, To, useLocation } from "react-router-dom"
import { PropsWithClassName } from "types"

type LinkFullscreenBaseProps = {
  location?: Location
  to: To
}

export type LinkFullscreenProps = PropsWithChildren & PropsWithClassName & LinkFullscreenBaseProps

export const LinkFullscreen = ({ children, className, location, to }: LinkFullscreenProps) => {
  const currentLocation = useLocation()

  return (
    <Link className={className} to={to} state={{ backgroundLocation: location ?? currentLocation }}>
      {children}
    </Link>
  )
}
