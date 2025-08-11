import { useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { TabsProvider } from "app"
import { TabContent, TabsList } from "ui/components"
import { GovernanceModerationHeader } from "ui/components/specific"

import { ReviewsTab } from "./ReviewsTab"
import { PublicationsTab } from "./PublicationsTab"
import { DiscussionsTab } from "./DiscussionsTab"
import { UsersTab } from "./UsersTab"

export const ModerationPage = () => {
  const { siteId, tabKey } = useParams()
  const { t } = useTranslation("moderation")
  const navigate = useNavigate()

  const handleTabSelect = useCallback((tab: string) => navigate(`/${siteId}/m/${tab}`), [navigate, siteId])

  return (
    <div className="flex flex-col gap-6">
      <GovernanceModerationHeader siteId={siteId!} title={t("title")} homeLabel={t("common:home")} />
      <TabsProvider defaultKey={tabKey || ""}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={[
              { key: "", label: t("reviews") },
              { key: "p", label: t("publications") },
              { key: "u", label: t("userRegistrations") },
              { key: "d", label: t("discussions") },
            ]}
          />

          <TabContent when="">
            <ReviewsTab />
          </TabContent>
          <TabContent when="p">
            <PublicationsTab />
          </TabContent>
          <TabContent when="u">
            <UsersTab />
          </TabContent>
          <TabContent when="d">
            <DiscussionsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </div>
  )
}
