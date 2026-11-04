import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import {
  useGetStoreAvatarChangeProposals,
  useGetStoreInfoUpdationProposals,
  useGetStoreRenamingProposals,
} from "entities"
import { useCanVoteOperation, useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { OperationType } from "types"
import { TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { ModerationHeader, OperationPolicyInfo } from "ui/components/specific"
import { routes, sortTabsByVoting } from "utils"

import { ProposalsOperationTab, ProposalsOperationTabProps } from "../ProposalsOperationTab"

const OPERATION_TABS: {
  key: string
  labelKey: string
  operation: OperationType
  useGetProposals: ProposalsOperationTabProps["useGetProposals"]
}[] = [
  {
    key: "renamings",
    labelKey: "renaming",
    operation: "store-renaming",
    useGetProposals: useGetStoreRenamingProposals,
  },
  {
    key: "info-updations",
    labelKey: "infoUpdation",
    operation: "store-info-updation",
    useGetProposals: useGetStoreInfoUpdationProposals,
  },
  {
    key: "avatar-changes",
    labelKey: "avatarChange",
    operation: "store-avatar-change",
    useGetProposals: useGetStoreAvatarChangeProposals,
  },
]

const routeToTabKey: Record<string, string> = Object.fromEntries(OPERATION_TABS.map(x => [x.key, x.key]))

export const StoreOperationsPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const { t } = useTranslation("storeOperationsPage")

  const canVoteOperation = useCanVoteOperation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Store")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.store(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string; operation?: OperationType })[] = useMemo(
    () =>
      sortTabsByVoting(
        OPERATION_TABS.map(x => ({ key: x.key, label: t(x.labelKey), route: x.key, operation: x.operation })),
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

          {OPERATION_TABS.map(x => (
            <TabContent key={x.key} when={x.key}>
              <OperationPolicyInfo operation={x.operation} />
              <ProposalsOperationTab
                operation={x.operation}
                useGetProposals={x.useGetProposals}
                getProposalPath={routes.moderation.storeProposal}
              />
            </TabContent>
          ))}
        </div>
      </TabsProvider>
    </>
  )
}
