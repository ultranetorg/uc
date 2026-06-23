import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useSiteContext } from "app"
import { useParams, useSiteTitle } from "hooks"
import { TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { routes } from "utils"

import { NewUsersTab } from "./NewUsersTab"
import { UsersRemovalsTab } from "./RemoveUsersTab"
import { UsersTab } from "./UsersTab"

const routeToTabKey: Record<string, string> = {
  n: "new-users",
  r: "users-removals",
  u: "users",
}

export const UsersPage = () => {
  const navigate = useNavigate()
  const { siteId, tabKey } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation("moderationUsersPage")

  useSiteTitle(site?.title, "Users")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.users(siteId!, item.route)),
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
