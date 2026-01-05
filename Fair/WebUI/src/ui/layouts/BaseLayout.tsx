import { memo, PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { PageBanner, Sidebar } from "ui/components/specific"

export const BaseLayout = memo(({ children }: PropsWithChildren) => (
  <div className="min-h-screen w-full">
    <PageBanner className="sticky top-0 z-20" />
    <div className="mx-auto max-w-[1440px]">
      <div className="flex min-h-screen w-full">
        <Sidebar />
        <div className="w-full">{children ?? <Outlet />}</div>
      </div>
    </div>
  </div>
))
