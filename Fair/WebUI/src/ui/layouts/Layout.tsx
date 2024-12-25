import { PropsWithChildren } from "react"
import { Outlet } from "react-router-dom"

import { Container, Footer, Navbar } from "ui/components"

export const Layout = ({ children }: PropsWithChildren) => {
  return (
    <div className="flex h-full flex-col">
      <Navbar />
      <Container className="flex-1">{children ?? <Outlet />}</Container>
      <Footer className="mt-10" />
    </div>
  )
}
