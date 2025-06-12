import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { ScrollToTop, SiteHeader } from "ui/components/specific"

export const SiteLayout = ({ children }: PropsWithChildren) => {
  return (
    <div className="p-6">
      <ScrollToTop />
      <SiteHeader />
      <div className="flex-1">{children ?? <Outlet />}</div>
    </div>
  )
}
