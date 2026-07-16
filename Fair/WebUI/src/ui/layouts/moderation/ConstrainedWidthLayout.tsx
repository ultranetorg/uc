import { memo, PropsWithChildren } from "react"
import { Outlet, useRouteError } from "react-router-dom"
import { twMerge } from "tailwind-merge"

export const ConstrainedWidthLayout = memo(({ children }: PropsWithChildren) => {
  const error = useRouteError()
  return (
    <div className={twMerge("flex flex-col gap-6", error ? "h-full" : "max-w-182.5")}>{children ?? <Outlet />}</div>
  )
})
