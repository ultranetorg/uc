import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { TabsProvider } from "app"
import { TabContent, TabsList } from "ui/components"
import { GovernanceModerationHeader } from "ui/components/specific"

import { ReviewsTab } from "./ReviewsTab"
import { PublicationsTab } from "./PublicationsTab"
import { DiscussionsTab } from "./DiscussionsTab"
import { UsersTab } from "./UsersTab"

export const ModerationPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("moderation")

  return (
    <div className="flex flex-col gap-6">
      <GovernanceModerationHeader siteId={siteId!} title={t("title")} homeLabel={t("common:home")} />
      <TabsProvider defaultKey="reviews">
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            items={[
              { key: "reviews", label: t("reviews") },
              { key: "publications", label: t("publications") },
              { key: "user-registrations", label: t("userRegistrations") },
              { key: "discussions", label: t("discussions") },
            ]}
          />

          <TabContent when="reviews">
            <ReviewsTab />
          </TabContent>
          <TabContent when="publications">
            <PublicationsTab />
          </TabContent>
          <TabContent when="user-registrations">
            <UsersTab />
          </TabContent>
          <TabContent when="discussions">
            <DiscussionsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </div>
  )
}
