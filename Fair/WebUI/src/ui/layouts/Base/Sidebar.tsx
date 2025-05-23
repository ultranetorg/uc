import { memo } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { Grid3x3GapFillSvg } from "assets"
import { PropsWithClassName } from "types"
import { PrimaryButton, SidebarSitesList } from "ui/components"
import { useTranslation } from "react-i18next"

const TEST_CURRENT_SITE: SidebarSite = {
  id: "0",
  title: "GameNest",
}

const TEST_SITES: SidebarSite[] = [
  { id: "1", title: "SoftVault" },
  { id: "2", title: "MovieMesh" },
  {
    id: "3",
    title:
      "This is very ery very very very ery very very very ery very very very ery very very very ery very very long site name",
  },
]

type SidebarSite = {
  id: string
  title: string
}

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("sidebar")

  return (
    <div className={twMerge("min-w-65 flex flex-col gap-6 p-2", className)}>
      <div className="flex flex-col gap-8 p-2">
        <Link to="/">
          <PrimaryButton
            className="w-full"
            image={<Grid3x3GapFillSvg className="fill-stone-800 stroke-stone-800" />}
            label="All Sites"
          />
        </Link>
        <SidebarSitesList title={t("currentSite")} items={[TEST_CURRENT_SITE]} />
        <SidebarSitesList title={t("starredSites")} items={TEST_SITES} />
      </div>
    </div>
  )
})
