import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import { useCanVoteOperation, useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { OperationType } from "types"
import { TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { OperationPolicyInfo } from "ui/components/specific"
import { routes, sortTabsByVoting } from "utils"

import { UsersNewTab } from "./tabs/UsersNewTab"
import { UsersRemovalsTab } from "./tabs/UsersRemovalsTab"
import { UsersListTab } from "./tabs/UsersListTab"

const routeToTabKey: Record<string, string> = {
  new: "new-users",
  remove: "users-removals",
  users: "users",
}

export const UsersPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const { t } = useTranslation("usersPage")

  const canVoteOperation = useCanVoteOperation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Users")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.users(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string; operation?: OperationType })[] = useMemo(
    () =>
      sortTabsByVoting(
        [
          { key: "new-users", label: t("common:new"), route: "new", operation: "user-registration" as const },
          {
            key: "users-removals",
            label: t("common:removals"),
            route: "remove",
            operation: "user-unregistration" as const,
          },
          { key: "users", label: t("all"), route: "users" },
        ],
        canVoteOperation,
      ),
    [canVoteOperation, t],
  )

  return (
    <TabsProvider key={key ? undefined : tabsItems[0].key} defaultKey={key || tabsItems[0].key}>
      <div className="flex flex-col gap-6">
        <TabsList
          className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
          itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
          activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
          onTabSelect={handleTabSelect}
          items={tabsItems}
        />

        <TabContent when="new-users">
          <OperationPolicyInfo operation="user-registration" />
          <UsersNewTab />
        </TabContent>
        <TabContent when="users-removals">
          <OperationPolicyInfo operation="user-unregistration" />
          <UsersRemovalsTab />
        </TabContent>
        <TabContent when="users">
          <UsersListTab />
        </TabContent>
      </div>
    </TabsProvider>
  )
}
