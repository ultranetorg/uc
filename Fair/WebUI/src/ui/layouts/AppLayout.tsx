import { memo, PropsWithChildren } from "react"
import { Outlet, Route, Routes, useLocation } from "react-router-dom"

import { ProfilePage } from "ui/pages"

export const AppLayout = memo(({ children }: PropsWithChildren) => {
  const location = useLocation()

  const state = location.state as { backgroundLocation?: Location }
  const backgroundLocation = state?.backgroundLocation
  const modalLocation = backgroundLocation || location
  const isFullscreenModal = !!backgroundLocation

  return (
    <>
      <Routes location={modalLocation}>
        <Route path="/" element={children ?? <Outlet />} />
      </Routes>

      {isFullscreenModal && (
        <Routes>
          <Route path="/profile/:id" element={<ProfilePage />} />
        </Routes>
      )}
    </>
  )
})
