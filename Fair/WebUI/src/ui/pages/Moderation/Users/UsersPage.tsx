import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { TabsProvider } from "app"
import { TabContent, TabsList, TabsListItem } from "ui/components"

import { NewUsersTab } from "./NewUsersTab"
import { UsersRemovalsTab } from "./UserRemovalsTab"
import { UsersTab } from "./UsersTab"

const routeToTabKey: Record<string, string> = {
  n: "new-users",
  r: "users-removals",
  u: "users",
}

export const UsersPage = () => {
  const navigate = useNavigate()
  const { siteId, tabKey } = useParams()
  const { t } = useTranslation("moderationUsersPage")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) =>
      navigate(item.route ? `/${siteId}/m/u/${item.route}` : `/${siteId}/m/u`),
    [navigate, siteId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "new-users", label: t("common:new"), route: "n" },
      { key: "users-removals", label: t("common:removals"), route: "r" },
      { key: "users", label: t("common:users") },
    ],
    [t],
  )

  return (
    <>
      <TabsProvider defaultKey={key || "new-users"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="new-users">
            <NewUsersTab />
          </TabContent>
          <TabContent when="users-removals">
            <UsersRemovalsTab />
          </TabContent>
          <TabContent when="users">
            <UsersTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
