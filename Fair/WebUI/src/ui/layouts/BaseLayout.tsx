import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

export const BaseLayout = ({ children }: PropsWithChildren) => (
  <div className="min-h-screen w-full">
    <div className="mx-auto max-w-[1440px] px-8">
      <div className="flex-1">{children ?? <Outlet />}</div>
    </div>
  </div>
)
