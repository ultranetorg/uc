import { memo, PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { PageBanner, Sidebar, SidebarDrawerProvider, Header } from "ui/components/specific"

export const BaseLayout = memo(({ children }: PropsWithChildren) => (
  <SidebarDrawerProvider>
    <div className="min-h-screen w-full">
      {/* A phone-sized banner wraps to several lines, so it only sticks to the top from `md`. */}
      <PageBanner className="z-20 md:sticky md:top-0" />
      <Header />
      <div className="mx-auto max-w-360">
        <div className="flex min-h-screen w-full">
          <Sidebar />
          <div className="w-full min-w-0">{children ?? <Outlet />}</div>
        </div>
      </div>
    </div>
  </SidebarDrawerProvider>
))
