import { useCallback, useMemo } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { TabsProvider } from "app"
import { TabContent, TabsList, TabsListItem } from "ui/components"
import { GovernanceModerationHeader } from "ui/components/specific"

import { PerpetualSurveysTab } from "./PerpetualSurveysTab"
import { ReferendumsTab } from "./ReferendumsTab"

const routeToTabKey: Record<string, string> = {
  p: "perpetual",
  r: "referendums",
}

export const ReferendumsPage = () => {
  const { siteId, tabKey } = useParams()
  const key = routeToTabKey[tabKey!]
  const { t } = useTranslation("governance")
  const navigate = useNavigate()

  const handleCreateButtonClick = useCallback(() => navigate(`/${siteId}/g/new`), [navigate, siteId])

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(item.route ? `/${siteId}/g/${item.route}` : `/${siteId}/g`),
    [navigate, siteId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "perpetual", label: t("perpetual") },
      { key: "referendums", label: t("referendums"), route: "r" },
    ],
    [t],
  )

  return (
    <div className="flex flex-col gap-6">
      <GovernanceModerationHeader
        proposalType="referendum"
        siteId={siteId!}
        title={t("title")}
        onCreateButtonClick={handleCreateButtonClick}
        homeLabel={t("common:home")}
        createButtonLabel={t("createReferendum")}
      />
      <TabsProvider defaultKey={key || "perpetual"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="perpetual">
            <PerpetualSurveysTab />
          </TabContent>
          <TabContent when="referendums">
            <ReferendumsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </div>
  )
}
