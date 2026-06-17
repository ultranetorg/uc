import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate, useParams } from "react-router-dom"

import { useOperationPolicy, useSiteContext } from "app"
import { useSiteTitle } from "hooks"
import { ModerationHeader } from "ui/components/specific"
import { ButtonPrimary, TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"

import { sitesKeys } from "entities"
import { routes } from "utils"
import { ModeratorsTab } from "./ModeratorsTab"
import { ModeratorsProposalsTab } from "./ModeratorsProposalsTab"

const routeToTabKey: Record<string, string> = {
  p: "proposals",
}

export const ModeratorsPage = () => {
  const navigate = useNavigate()
  const { voterId } = useOperationPolicy("site-moderator-addition")
  const { siteId, tabKey } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation("moderatorsPage")

  useSiteTitle(site?.title, "Moderators")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.moderators(siteId!, item.route)),
    [navigate, siteId],
  )

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
        components={
          <>
            {voterId && (
              <Link
                to={routes.governance.create(siteId!)}
                state={{
                  parentBreadcrumbs: [{ path: routes.moderation.moderators(siteId!), title: t("title") }],
                  title: t("addModerator"),
                  type: "site-moderator-addition",
                  redirectAfterProposalCreation: routes.moderation.moderators(siteId!, "p"),
                  redirectAfterProposalExecution: location.pathname,
                  invalidateQueryKeys: sitesKeys.publishers(siteId!),
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
