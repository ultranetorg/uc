import { memo, PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

export const ModerationLayout = memo(({ children }: PropsWithChildren) => (
  <div className="flex flex-col gap-6">{children ?? <Outlet />}</div>
))
