import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { Sidebar } from "./Sidebar"
import { SiteHeader } from "./SiteHeader"

export const SiteLayout = ({ children }: PropsWithChildren) => {
  return (
    <div className="flex min-h-screen w-full divide-x divide-zinc-300">
      <Sidebar className="min-w-52 pr-8" />
      <div className="w-full pl-8">
        <SiteHeader />
        <div className="flex-1">{children ?? <Outlet />}</div>
      </div>
    </div>
  )
}
