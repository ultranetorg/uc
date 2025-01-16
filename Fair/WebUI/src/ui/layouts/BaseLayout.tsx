import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

export const BaseLayout = ({ children }: PropsWithChildren) => (
  <div>
    <div>BaseLayout</div>
    {children ?? <Outlet />}
  </div>
)
