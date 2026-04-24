import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate, useParams } from "react-router-dom"

import { TabsProvider, useModerationContext } from "app"
import { ModerationHeader } from "ui/components/specific"
import { ButtonPrimary, TabContent, TabsList, TabsListItem } from "ui/components"

import { ModeratorsTab } from "./ModeratorsTab"
import { ModeratorsProposalsTab } from "./ModeratorsProposalsTab"

const routeToTabKey: Record<string, string> = {
  p: "proposals",
}

export const ModeratorsPage = () => {
  const navigate = useNavigate()
  const { getOperationVoterId } = useModerationContext()
  const { siteId, tabKey } = useParams()
  const { t } = useTranslation("moderatorsPage")

  const voterId = getOperationVoterId("site-moderator-addition")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) =>
      navigate(item.route ? `/${siteId}/m/m/${item.route}` : `/${siteId}/m/m`),
    [navigate, siteId],
  )

  const parentBreadcrumbs = useMemo(() => [{ path: `/${siteId}/m`, title: t("common:proposals") }], [siteId, t])

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "moderators", label: t("common:moderators") },
      { key: "proposals", label: t("common:proposals"), route: "p" },
    ],
    [t],
  )

  return (
    <>
      <ModerationHeader
        title={t("title")}
        parentBreadcrumbs={parentBreadcrumbs}
        components={
          <>
            {voterId && (
              <Link
                to={`/${siteId}/g/new`}
                state={{
                  parentBreadcrumbs: [...parentBreadcrumbs, { path: `/${siteId}/m/m/`, title: t("title") }],
                  title: t("addModerator"),
                  type: "site-moderator-addition",
                  previousPath: `/${siteId}/m/m/`,
                }}
              >
                <ButtonPrimary label={t("addModerator")} />
              </Link>
            )}
          </>
        }
      />
      <TabsProvider defaultKey={key || "moderators"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="moderators">
            <ModeratorsTab />
          </TabContent>
          <TabContent when="proposals">
            <ModeratorsProposalsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
