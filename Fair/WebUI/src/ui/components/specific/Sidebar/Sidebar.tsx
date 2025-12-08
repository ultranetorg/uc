import { memo } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useAccountsContext, useSiteContext } from "app"
import { PropsWithClassName } from "types"
import { SitesList } from "ui/components/sidebar"
import { CurrentAccount } from "ui/components/specific"

import { AllSitesButton } from "./components"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("sites")

  const { site } = useSiteContext()
  const { currentAccount } = useAccountsContext()

  return (
    <div className={twMerge("flex w-65 min-w-65 flex-col gap-6 p-2", className)}>
      <div className="flex grow flex-col gap-8 p-2">
        <Link to="/">
          <AllSitesButton title={t("allSites")} />
        </Link>
        {site && <SitesList title={t("currentSite")} items={[site]} emptyStateMessage={t("emptySitesList")} />}
        <SitesList
          title={t("starredSites")}
          items={currentAccount?.favoriteSites}
          emptyStateMessage={t("emptySitesList")}
        />
      </div>
      <CurrentAccount />
    </div>
  )
})
