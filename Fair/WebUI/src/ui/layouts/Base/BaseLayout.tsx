import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { Sidebar } from "ui/components/specific"

export const BaseLayout = ({ children }: PropsWithChildren) => (
  <div className="min-h-screen w-full">
    <div className="mx-auto max-w-[1440px]">
      <div className="flex min-h-screen w-full">
        <Sidebar />
        <div className="w-full">{children ?? <Outlet />}</div>
      </div>
    </div>
  </div>
)
