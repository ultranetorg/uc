import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, Navigate } from "react-router-dom"

import { useAuthenticationContext } from "app"
import { SvgProfilePageClose } from "assets"
import { ProfileTabs } from "ui/components/profile"
import { useCloseFullscreen, useEscapeKey, useLocationState, useSiteTitle } from "hooks"
import { routes } from "utils"

export const ProfilePage = () => {
  const { t } = useTranslation("profile")

  const [titleKey, setTitleKey] = useState("profile")

  const { backgroundLocation, defaultTabKey } = useLocationState()

  const { selectedUserName } = useAuthenticationContext()

  useSiteTitle(`Profile - ${selectedUserName}`)

  const close = useCloseFullscreen()

  useEscapeKey(close)

  const handleTabSelect = useCallback((tab: string) => setTitleKey(tab), [])

  if (!selectedUserName) {
    return <Navigate to={routes.home()} replace={true} />
  }

  return (
    <div className="fixed inset-0 z-50 bg-white">
      <div className="mx-auto max-w-[1240px]">
        <div className="flex pl-17">
          <div className="flex w-full gap-6">
            <div className="flex w-full flex-col gap-6 py-8">
              <span className="text-3.5xl font-semibold leading-10">{t(titleKey)}</span>
              <ProfileTabs
                tabsListClassName="w-87.5 h-fit"
                onTabSelect={handleTabSelect}
                defaultTabKey={defaultTabKey}
              />
            </div>
            <div className="pt-7.5">
              {backgroundLocation ? (
                <SvgProfilePageClose className="cursor-pointer" onClick={close} />
              ) : (
                <Link to={routes.home()}>
                  <SvgProfilePageClose className="cursor-pointer" />
                </Link>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
