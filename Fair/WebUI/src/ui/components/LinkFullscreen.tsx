import { PropsWithChildren } from "react"
import { Link, To, useLocation } from "react-router-dom"

type LinkFullscreenBaseProps = {
  location?: Location
  to: To
}

export type LinkFullscreenProps = PropsWithChildren & LinkFullscreenBaseProps

export const LinkFullscreen = ({ children, location, to }: LinkFullscreenProps) => {
  const currentLocation = useLocation()

  return (
    <Link to={to} state={{ backgroundLocation: location ?? currentLocation }}>
      {children}
    </Link>
  )
}
