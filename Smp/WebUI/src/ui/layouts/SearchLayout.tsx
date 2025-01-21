import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

export const SearchLayout = ({ children }: PropsWithChildren) => (
  <div>
    <div>SearchLayout</div>
    {children ?? <Outlet />}
  </div>
)
