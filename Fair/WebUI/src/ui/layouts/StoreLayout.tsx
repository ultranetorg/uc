import { PropsWithChildren } from "react"
import { Outlet, useRouteError } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { ScrollToTop, StoreHeader } from "ui/components/specific"

export const StoreLayout = ({ children }: PropsWithChildren) => {
  const error = useRouteError()

  return (
    <div className={twMerge("p-6", !!error && "flex h-full flex-col")}>
      <ScrollToTop />
      <StoreHeader />
      <div className="flex-1">{children ?? <Outlet />}</div>
    </div>
  )
}
