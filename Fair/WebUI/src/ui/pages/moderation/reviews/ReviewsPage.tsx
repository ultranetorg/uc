import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { routes } from "utils"

import { ReviewCreationsTab } from "./ReviewCreationsTab"
import { ReviewDeletionsTab } from "./ReviewDeletionsTab"

const routeToTabKey: Record<string, string> = {
  new: "new",
  deletions: "deletions",
}

export const ReviewsPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation("reviewsPage")

  useStoreTitle(store?.title, "Reviews")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.reviews(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "new", label: t("common:new") },
      { key: "deletions", label: t("common:deletions"), route: "deletions" },
    ],
    [t],
  )

  return (
    <>
      <ModerationHeader title={t("title")} showModeratorsCount={true} />
      <TabsProvider defaultKey={key || "new"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="new">
            <ReviewCreationsTab />
          </TabContent>
          <TabContent when="deletions">
            <ReviewDeletionsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
