import { useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useLocation, useNavigate } from "react-router-dom"

import { SvgProfilePageClose } from "assets"
import { ProfileTabs } from "ui/components/profile"

const CLOSE_CLASSNAME = "cursor-pointer"

export const ProfilePage = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const { t } = useTranslation("profile")

  const [titleKey, setTitleKey] = useState("profile")

  const state = location.state as { backgroundLocation?: Location } | undefined
  const backgroundLocation = state?.backgroundLocation

  const close = useCallback(() => {
    navigate(-1)
  }, [navigate])

  const handleTabSelect = useCallback((tab: string) => setTitleKey(tab), [])

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        close()
      }
    }

    document.addEventListener("keydown", handleKeyDown)

    return () => {
      document.removeEventListener("keydown", handleKeyDown)
    }
  }, [close])

  return (
    <div className="fixed inset-0 z-50 bg-white">
      <div className="mx-auto max-w-[1240px]">
        <div className="flex pl-17">
          <div className="flex w-full gap-6">
            <div className="flex w-full flex-col gap-6 py-8">
              <span className="text-3.5xl font-semibold leading-10">{t(titleKey)}</span>
              <ProfileTabs tabsListClassName="w-87.5 h-fit" onTabSelect={handleTabSelect} />
            </div>
            <div className="pt-7.5">
              {backgroundLocation ? (
                <SvgProfilePageClose className={CLOSE_CLASSNAME} onClick={close} />
              ) : (
                <Link to={`/`}>
                  <SvgProfilePageClose className={CLOSE_CLASSNAME} />
                </Link>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
