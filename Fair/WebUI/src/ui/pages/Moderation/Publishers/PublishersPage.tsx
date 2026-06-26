import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useSiteContext } from "app"
import { useParams, useResolveSiteId, useSiteTitle } from "hooks"
import { ModerationHeader } from "ui/components/specific"
import { TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { routes } from "utils"

import { PublishersTab } from "./PublishersTab"
import { PublishersProposalsTab } from "./PublishersProposalsTab"

const routeToTabKey: Record<string, string> = {
  publishers: "publishers",
  proposals: "proposals",
}

export const PublishersPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const siteId = useResolveSiteId()
  const { site } = useSiteContext()
  const { t } = useTranslation("publishersPage")

  useSiteTitle(site?.title, "Publishers")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.publishers(siteId!, item.route)),
    [navigate, siteId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "publishers", label: t("common:publishers") },
      { key: "proposals", label: t("common:proposals"), route: "proposals" },
    ],
    [t],
  )

  return (
    <>
      <ModerationHeader title={t("title")} />
      <TabsProvider defaultKey={key || "publishers"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="publishers">
            <PublishersTab />
          </TabContent>
          <TabContent when="proposals">
            <PublishersProposalsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
