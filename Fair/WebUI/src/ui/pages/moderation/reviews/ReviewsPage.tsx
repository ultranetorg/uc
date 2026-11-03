import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import { useGetReviewCreationsProposals, useGetReviewEditsProposals } from "entities"
import { useCanVoteOperation, useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { OperationType } from "types"
import { TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { ModerationHeader, OperationPolicyInfo } from "ui/components/specific"
import { routes, sortTabsByVoting } from "utils"

import { ReviewsDeletionsTab } from "./ReviewsDeletionsTab"
import { ReviewsProposalsTab, ReviewsProposalsTabProps } from "./ReviewsProposalsTab"

const PROPOSALS_TABS: {
  key: string
  labelKey: string
  operation: OperationType
  useGetProposals: ReviewsProposalsTabProps["useGetProposals"]
}[] = [
  {
    key: "creations",
    labelKey: "creation",
    operation: "review-creation",
    useGetProposals: useGetReviewCreationsProposals,
  },
  { key: "edits", labelKey: "edit", operation: "review-edit", useGetProposals: useGetReviewEditsProposals },
]

const routeToTabKey: Record<string, string> = {
  ...Object.fromEntries(PROPOSALS_TABS.map(x => [x.key, x.key])),
  deletions: "deletions",
}

export const ReviewsPage = () => {
  const navigate = useNavigate()
  const { tabKey } = useParams()
  const { t } = useTranslation("reviewsPage")

  const canVoteOperation = useCanVoteOperation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Reviews")

  const key = routeToTabKey[tabKey!]

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.reviews(storeId!, item.route)),
    [navigate, storeId],
  )

  const tabsItems: (TabsListItem & { route?: string; operation?: OperationType })[] = useMemo(
    () =>
      sortTabsByVoting(
        [
          ...PROPOSALS_TABS.map(x => ({ key: x.key, label: t(x.labelKey), route: x.key, operation: x.operation })),
          { key: "deletions", label: t("deletion"), route: "deletions", operation: "review-deletion" as const },
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

          {PROPOSALS_TABS.map(x => (
            <TabContent key={x.key} when={x.key}>
              <OperationPolicyInfo operation={x.operation} />
              <ReviewsProposalsTab useGetProposals={x.useGetProposals} />
            </TabContent>
          ))}
          <TabContent when="deletions">
            <OperationPolicyInfo operation="review-deletion" />
            <ReviewsDeletionsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
