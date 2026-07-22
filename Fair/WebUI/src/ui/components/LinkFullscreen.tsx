import { memo, PropsWithChildren } from "react"
import { Link, LinkProps, To, useLocation } from "react-router-dom"

import { useResolveStoreId } from "hooks"
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

export const LinkFullscreen = memo(({ children, className, title, location, to, params }: LinkFullscreenProps) => {
  const currentLocation = useLocation()
  const storeId = useResolveStoreId()

  return (
    <Link
      className={className}
      to={to}
      state={{ backgroundLocation: location ?? currentLocation, siteId: storeId, ...params } as LinkFullscreenState}
      title={title}
    >
      {children}
    </Link>
  )
})
