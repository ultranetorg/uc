import { memo, PropsWithChildren } from "react"
import { Outlet, Route, Routes, useLocation } from "react-router-dom"

import { ProfilePage } from "ui/pages"

export const AppLayout = memo(({ children }: PropsWithChildren) => {
  const location = useLocation()

  const { backgroundLocation } = location.state as { backgroundLocation?: Location }
  const hasFullscreenModal = !!backgroundLocation

  return (
    <>
      <Routes location={backgroundLocation || location}>
        <Route path="/" element={children ?? <Outlet />} />
      </Routes>

      {hasFullscreenModal && (
        <Routes>
          <Route path="/profile/:id" element={<ProfilePage />} />
        </Routes>
      )}
    </>
  )
})
