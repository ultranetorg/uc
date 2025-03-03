import { PropsWithChildren } from "react"
import { Link, Outlet } from "react-router-dom"

export const BaseLayout = ({ children }: PropsWithChildren) => (
  <div className="min-h-screen w-full p-12">
    <div className="absolute right-0 top-0 text-5xl">
      <Link to="/">👤 user</Link>
    </div>
    <div className="flex-1">{children ?? <Outlet />}</div>
  </div>
)
