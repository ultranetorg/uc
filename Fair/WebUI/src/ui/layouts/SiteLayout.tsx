import { PropsWithChildren } from "react"
import { Outlet, useRouteError } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { ScrollToTop, SiteHeader } from "ui/components/specific"

export const SiteLayout = ({ children }: PropsWithChildren) => {
  const error = useRouteError()

  return (
    <div className={twMerge("p-6", !!error && "flex h-full flex-col")}>
      <ScrollToTop />
      <SiteHeader />
      <div className="flex-1">{children ?? <Outlet />}</div>
    </div>
  )
}
