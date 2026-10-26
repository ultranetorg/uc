import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "react-router-dom"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ModerationHeader } from "ui/components/specific"
import { ButtonPrimary, TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"

import { isModeratorCanCreate, isPublisherCanCreate, routes } from "utils"

import { AddModeratorsTab } from "./AddModeratorsTab"
import { ModeratorsTab } from "./ModeratorsTab"
import { RemoveModeratorsTab } from "./RemoveModeratorsTab"

const routeToTabKey: Record<string, string> = {
  moderators: "moderators",
  removals: "removals",
  additions: "additions",
}

export const ModeratorsPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { t } = useTranslation("moderatorsPage")

  useStoreTitle(store?.title, "Moderators")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.moderators(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "moderators", label: t("common:moderators"), route: "moderators" },
      { key: "removals", label: t("common:removals"), route: "removals" },
      { key: "additions", label: t("common:additions"), route: "additions" },
    ],
    [t],
  )

  const canAddModerator =
    (isPublisher && isPublisherCanCreate("store-moderator-addition", policies)) ||
    (isModerator && isModeratorCanCreate("store-moderator-addition", policies))

  return (
    <>
      <ModerationHeader
        title={t("title")}
        components={
          <>
            {canAddModerator && (
              <Link
                to={routes.moderation.createProposal(storeId!)}
                state={{
                  parentBreadcrumbs: [{ path: routes.moderation.moderators(storeId!), title: t("title") }],
                  title: t("addModerator"),
                  type: "store-moderator-addition",
                  redirectAfterProposalCreation: routes.moderation.moderators(storeId!, "p"),
                  redirectAfterProposalExecution: location.pathname,
                }}
              >
                <ButtonPrimary label={t("addModerator")} />
              </Link>
            )}
          </>
        }
        showModeratorsCount={true}
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
          <TabContent when="removals">
            <RemoveModeratorsTab />
          </TabContent>
          <TabContent when="additions">
            <AddModeratorsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
