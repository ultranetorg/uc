import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import { useCanVoteOperation, useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { OperationType } from "types"
import { ModerationHeader, OperationPolicyInfo } from "ui/components/specific"
import { TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { routes, sortTabsByVoting } from "utils"

import { PublishersListTab } from "./PublishersListTab"
import { PublishersRemovalsTab } from "./PublishersRemovalsTab"

const routeToTabKey: Record<string, string> = {
  publishers: "publishers",
  removals: "removals",
}

export const PublishersPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation("publishersPage")

  useStoreTitle(store?.title, "Publishers")

  const canVoteOperation = useCanVoteOperation()

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.publishers(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string; operation?: OperationType })[] = useMemo(
    () =>
      sortTabsByVoting(
        [
          { key: "publishers", label: t("all"), route: "publishers" },
          {
            key: "removals",
            label: t("expel"),
            route: "removals",
            operation: "store-authors-removal" as const,
          },
        ],
        canVoteOperation,
      ),
    [canVoteOperation, t],
  )

  return (
    <>
      <ModerationHeader title={t("title")} />
      <TabsProvider key={key ? undefined : tabsItems[0].key} defaultKey={key || tabsItems[0].key}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="publishers">
            <PublishersListTab />
          </TabContent>
          <TabContent when="removals">
            <OperationPolicyInfo operation="store-authors-removal" />
            <PublishersRemovalsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
