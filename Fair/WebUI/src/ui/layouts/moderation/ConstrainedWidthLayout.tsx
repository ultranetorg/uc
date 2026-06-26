import { memo, PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

export const ConstrainedWidthLayout = memo(({ children }: PropsWithChildren) => (
  <div className="flex max-w-182.5 flex-col gap-6">{children ?? <Outlet />}</div>
))
