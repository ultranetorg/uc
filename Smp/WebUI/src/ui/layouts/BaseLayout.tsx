import { PropsWithChildren } from "react"
import { Link, Outlet } from "react-router-dom"

export const BaseLayout = ({ children }: PropsWithChildren) => (
  <div className="min-h-screen w-full p-12">
    <div className="flex w-full items-center justify-between gap-16 bg-gray-400">
      <h1>
        <Link to="/">🏡 MY STORE</Link>
      </h1>
      <h1>
        <Link to="/p">🔍 Search</Link>
      </h1>
    </div>
    <div className="flex-1">{children ?? <Outlet />}</div>
  </div>
)
