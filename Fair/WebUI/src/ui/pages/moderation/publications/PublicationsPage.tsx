import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import {
  useGetPublicationCreationProposals,
  useGetPublicationDeletionProposals,
  useGetPublicationPublishProposals,
  useGetPublicationUnpublishProposals,
  useGetPublicationUpdationProposals,
} from "entities"
import { useCanCreateOperation, useCanVoteOperation, useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { OperationType } from "types"
import { ButtonPrimary, TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { ModerationHeader, OperationPolicyInfo } from "ui/components/specific"
import { routes, sortTabsByVoting } from "utils"

import { PublicationsChangedTab } from "./tabs/PublicationsChangedTab"
import { PublicationsOperationTab, PublicationsOperationTabProps } from "./tabs/PublicationsOperationTab"
import { PublicationsUnpublishedTab } from "./tabs/PublicationsUnpublishedTab"

const OPERATION_TABS: {
  key: string
  operation: OperationType
  labelKey: string
  useGetProposals: PublicationsOperationTabProps["useGetProposals"]
}[] = [
  {
    key: "creations",
    operation: "publication-creation",
    labelKey: "creation",
    useGetProposals: useGetPublicationCreationProposals,
  },
  {
    key: "deletions",
    operation: "publication-deletion",
    labelKey: "deletion",
    useGetProposals: useGetPublicationDeletionProposals,
  },
  {
    key: "publishes",
    operation: "publication-publish",
    labelKey: "publish",
    useGetProposals: useGetPublicationPublishProposals,
  },
  {
    key: "unpublishes",
    operation: "publication-unpublish",
    labelKey: "unpublish",
    useGetProposals: useGetPublicationUnpublishProposals,
  },
  {
    key: "updations",
    operation: "publication-updation",
    labelKey: "updation",
    useGetProposals: useGetPublicationUpdationProposals,
  },
]

const routeToTabKey: Record<string, string> = {
  ...Object.fromEntries(OPERATION_TABS.map(x => [x.key, x.key])),
  changed: "changed",
  unpublished: "unpublished",
}

export const PublicationsPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const { t } = useTranslation("publicationsPage")

  const canCreateOperation = useCanCreateOperation()
  const canVoteOperation = useCanVoteOperation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Publications")

  const key = routeToTabKey[tabKey!]

  const handleSearchProduct = useCallback(
    () => navigate(routes.moderation.createPublication(storeId!)),
    [navigate, storeId],
  )

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.publications(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string; operation?: OperationType })[] = useMemo(
    () =>
      sortTabsByVoting<TabsListItem & { route?: string; operation?: OperationType }>(
        [
          ...OPERATION_TABS.map(x => ({ key: x.key, label: t(x.labelKey), route: x.key, operation: x.operation })),
          { key: "changed", label: t("changed"), route: "changed" },
          { key: "unpublished", label: t("unpublished"), route: "unpublished" },
        ],
        canVoteOperation,
      ),
    [canVoteOperation, t],
  )

  return (
    <>
      <ModerationHeader
        title={t("title")}
        components={
          <>
            {canCreateOperation("publication-creation") && (
              <ButtonPrimary className="w-48" label={t("searchProduct")} onClick={handleSearchProduct} />
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

          {OPERATION_TABS.map(x => (
            <TabContent key={x.key} when={x.key}>
              <OperationPolicyInfo operation={x.operation} />
              <PublicationsOperationTab operation={x.operation} useGetProposals={x.useGetProposals} />
            </TabContent>
          ))}
          <TabContent when="changed">
            <PublicationsChangedTab />
          </TabContent>
          <TabContent when="unpublished">
            <PublicationsUnpublishedTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
