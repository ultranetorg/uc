import { createContext, memo, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react"
import { useLocation } from "react-router-dom"
import { useMediaQuery } from "usehooks-ts"

import { useEscapeKey } from "hooks"

// Matches Tailwind's `lg`: from this width the sidebar is a permanent column, below it - an off-canvas drawer.
const DESKTOP_MEDIA_QUERY = "(min-width: 64rem)"

type SidebarDrawerContextType = {
  isOpen: boolean
  hasSidebar: boolean
  setHasSidebar: (value: boolean) => void
  close: () => void
  toggle: () => void
}

const SidebarDrawerContext = createContext<SidebarDrawerContextType>({
  isOpen: false,
  hasSidebar: false,
  setHasSidebar: () => {},
  close: () => {},
  toggle: () => {},
})

export const SidebarDrawerProvider = memo(({ children }: PropsWithChildren) => {
  const { pathname } = useLocation()

  const [openedPath, setOpenedPath] = useState<string | undefined>()
  const [hasSidebar, setHasSidebar] = useState(false)

  const isDesktop = useMediaQuery(DESKTOP_MEDIA_QUERY)
  const isOpen = openedPath === pathname

  const close = useCallback(() => setOpenedPath(undefined), [])
  const toggle = useCallback(() => setOpenedPath(prev => (prev === pathname ? undefined : pathname)), [pathname])

  useEscapeKey(isOpen ? close : undefined)

  useEffect(() => {
    if (isDesktop) close()
  }, [isDesktop, close])

  useEffect(() => {
    if (!isOpen) return undefined

    const { overflow } = document.body.style
    document.body.style.overflow = "hidden"
    return () => {
      document.body.style.overflow = overflow
    }
  }, [isOpen])

  const value = useMemo<SidebarDrawerContextType>(
    () => ({ isOpen, hasSidebar, setHasSidebar, close, toggle }),
    [isOpen, hasSidebar, close, toggle],
  )

  return <SidebarDrawerContext.Provider value={value}>{children}</SidebarDrawerContext.Provider>
})

// eslint-disable-next-line react-refresh/only-export-components
export const useSidebarDrawer = () => useContext(SidebarDrawerContext)
