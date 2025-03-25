import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

export const BaseLayout = ({ children }: PropsWithChildren) => (
  <div className="min-h-screen w-full p-12">
    <div className="flex-1">{children ?? <Outlet />}</div>
  </div>
)
