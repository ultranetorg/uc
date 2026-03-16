import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { TabsProvider } from "app"
import { ModerationHeader } from "ui/components/specific"
import { ButtonPrimary, TabContent, TabsList, TabsListItem } from "ui/components"
import { PublishersTab } from "./PublishersTab"
import { PublishersProposalsTab } from "./PublishersProposalsTab"

const routeToTabKey: Record<string, string> = {
  p: "publishers",
  r: "proposals",
}

export const PublishersPage = () => {
  const navigate = useNavigate()
  const { siteId, tabKey } = useParams()
  const { t } = useTranslation("publishersPage")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) =>
      navigate(item.route ? `/${siteId}/m/a/${item.route}` : `/${siteId}/m/a`),
    [navigate, siteId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "publishers", label: t("common:publishers"), route: "p" },
      { key: "proposals", label: t("common:proposals"), route: "r" },
    ],
    [t],
  )

  return (
    <>
      <ModerationHeader
        title={t("title")}
        parentBreadcrumbs={{ path: `/${siteId}/m`, title: t("common:proposals") }}
        components={<ButtonPrimary label={t("addPublisher")} />}
      />
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
