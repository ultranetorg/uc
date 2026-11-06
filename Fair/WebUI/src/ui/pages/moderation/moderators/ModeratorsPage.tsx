import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "react-router-dom"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { useCanVoteOperation, useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { OperationType } from "types"
import { ModerationHeader, OperationPolicyInfo } from "ui/components/specific"
import { ButtonPrimary, TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { isModeratorCanCreate, isPublisherCanCreate, routes, sortTabsByVoting } from "utils"

import { ModeratorsAdditionsTab } from "./tabs/ModeratorsAdditionsTab"
import { ModeratorsListTab } from "./tabs/ModeratorsListTab"
import { ModeratorsRemovalsTab } from "./tabs/ModeratorsRemovalsTab"

const routeToTabKey: Record<string, string> = {
  moderators: "moderators",
  removals: "removals",
  additions: "additions",
}

export const ModeratorsPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const { t } = useTranslation("moderatorsPage")

  const canVoteOperation = useCanVoteOperation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()

  useStoreTitle(store?.title, "Moderators")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.moderators(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string; operation?: OperationType })[] = useMemo(
    () =>
      sortTabsByVoting(
        [
          { key: "moderators", label: t("all"), route: "moderators" },
          {
            key: "removals",
            label: t("recall"),
            route: "removals",
            operation: "store-moderator-removal" as const,
          },
          {
            key: "additions",
            label: t("new"),
            route: "additions",
            operation: "store-moderator-addition" as const,
          },
        ],
        canVoteOperation,
      ),
    [canVoteOperation, t],
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
      />
      <TabsProvider key={key ? undefined : tabsItems[0].key} defaultKey={key || tabsItems[0].key}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="moderators">
            <ModeratorsListTab />
          </TabContent>
          <TabContent when="removals">
            <OperationPolicyInfo operation="store-moderator-removal" />
            <ModeratorsRemovalsTab />
          </TabContent>
          <TabContent when="additions">
            <OperationPolicyInfo operation="store-moderator-addition" />
            <ModeratorsAdditionsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
