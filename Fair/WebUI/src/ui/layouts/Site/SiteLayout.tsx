import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { SiteHeader } from "ui/components"

export const SiteLayout = ({ children }: PropsWithChildren) => {
  return (
    <div className="w-full pl-8">
      <SiteHeader />
      <div className="flex-1">{children ?? <Outlet />}</div>
    </div>
  )
}
