import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { TabsProvider } from "app"
import { TabContent, TabsList, TabsListItem } from "ui/components"
import { GovernanceModerationHeader } from "ui/components/specific"

import { ChangedPublicationsTab } from "./ChangedPublicationsTab"
import { DiscussionsTab } from "./DiscussionsTab"
import { PublicationsTab } from "./PublicationsTab"
import { ReviewsTab } from "./ReviewsTab"
import { UnpublishedPublicationsTab } from "./UnpublishedPublicationsTab"
import { UserRegistrationsTab } from "./UserRegistrationsTab"

const routeToTabKey: Record<string, string> = {
  p: "publications",
  u: "user-registrations",
  r: "reviews",
  n: "unpublished-publications",
  c: "changed-publications",
}

export const ModerationPage = () => {
  const { siteId, tabKey } = useParams()
  const key = routeToTabKey[tabKey!]
  const { t } = useTranslation("moderation")
  const navigate = useNavigate()

  const handleCreateButtonClick = useCallback(() => navigate(`/${siteId}/m/new`), [navigate, siteId])

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(item.route ? `/${siteId}/m/${item.route}` : `/${siteId}/m`),
    [navigate, siteId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "discussions", label: t("discussions") },
      { key: "changed-publications", label: t("changedPublications"), route: "c" },
      { key: "unpublished-publications", label: t("unpublishedPublications"), route: "n" },
      { key: "publications", label: t("publications"), route: "p" },
      { key: "reviews", label: t("reviews"), route: "r" },
      { key: "user-registrations", label: t("userRegistrations"), route: "u" },
    ],
    [t],
  )

  return (
    <div className="flex flex-col gap-6">
      <GovernanceModerationHeader
        siteId={siteId!}
        title={t("title")}
        onCreateButtonClick={handleCreateButtonClick}
        homeLabel={t("common:home")}
        createButtonLabel={t("createDiscussion")}
      />
      <TabsProvider defaultKey={key || "discussions"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="changed-publications">
            <ChangedPublicationsTab />
          </TabContent>
          <TabContent when="discussions">
            <DiscussionsTab />
          </TabContent>
          <TabContent when="publications">
            <PublicationsTab />
          </TabContent>
          <TabContent when="reviews">
            <ReviewsTab />
          </TabContent>
          <TabContent when="unpublished-publications">
            <UnpublishedPublicationsTab />
          </TabContent>
          <TabContent when="user-registrations">
            <UserRegistrationsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </div>
  )
}
