import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { Sidebar } from "./Sidebar"

export const BaseLayout = ({ children }: PropsWithChildren) => (
  <div className="min-h-screen w-full">
    <div className="mx-auto max-w-[1440px] px-8">
      <div className="flex min-h-screen w-full">
        <Sidebar className="w-61" />
        <div className="w-full pl-8">{children ?? <Outlet />}</div>
      </div>
    </div>
  </div>
)
