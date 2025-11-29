import { memo } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName, SiteBase } from "types"
import { SitesList } from "ui/components/sidebar"
import { CurrentAccount } from "ui/components/specific"

import { AllSitesButton } from "./components"

const TEST_CURRENT_SITE: SiteBase = {
  id: "0",
  title: "GameNest",
}

const TEST_SITES: SiteBase[] = [
  { id: "1", title: "SoftVault" },
  { id: "2", title: "MovieMesh" },
  {
    id: "3",
    title:
      "This is very ery very very very ery very very very ery very very very ery very very very ery very very long site name",
  },
]

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("sites")

  return (
    <div className={twMerge("flex min-w-65 flex-col gap-6 p-2", className)}>
      <div className="flex grow flex-col gap-8 p-2">
        <Link to="/">
          <AllSitesButton title={t("allSites")} />
        </Link>
        <SitesList title={t("currentSite")} items={[TEST_CURRENT_SITE]} emptyStateMessage={t("emptySitesList")} />
        <SitesList title={t("starredSites")} items={TEST_SITES} emptyStateMessage={t("emptySitesList")} />
      </div>
      <CurrentAccount />
    </div>
  )
})
