import { ReactNode, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetUserUnregistrationProposals, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useApprovalIdentityForOperation, useResolveStoreId, useUrlParamsState } from "hooks"
import { ProposalVoting, UserUnregistrationProposal } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import {
  renderActions,
  renderCommon,
  renderCreator,
  renderProposalProgress,
  renderUser,
  renderWithYouBadge,
  renderYourVote,
} from "ui/renderers"
import { calculateVotesRequiredToWinProposal, isUserVoted, parseInteger, showToast } from "utils"

const getItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string, name: string) => void,
    onReject: (id: string, name: string) => void,
    votesRequired: number,
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
    currentUserId?: string,
    userId?: string,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as UserUnregistrationProposal
    const isVoted = isUserVoted(currentUserId, proposal)

    switch (column.type) {
      case "user":
        return renderWithYouBadge(t, renderUser(proposal.userId, proposal.userName), proposal.userId === userId)

      case "created-by":
        return renderCreator(proposal.creator)

      case "progress":
        return renderProposalProgress(t, proposal, votesRequired)

      case "your-vote":
        return renderYourVote(t, proposal, currentUserId ? [currentUserId] : [])

      case "actions":
        return renderActions(
          t,
          () => onApprove(item.id, proposal.creator.title),
          () => onReject(item.id, proposal.creator.title),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          isVoted || (loadingItem && loadingItem.id !== item.id),
        )
    }

    return renderCommon(t, column, proposal, votesRequired)
  }

export const UsersRemovalsTab = () => {
  const storeId = useResolveStoreId()
  const approval = useApprovalIdentityForOperation("user-unregistration")
  const { invalidateOperation } = useInvalidation()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { user } = useUserContext()
  const { t } = useTranslation("usersPage")

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })
  const [page, setPage] = useState(state.page)
  const [loadingItem, setLoadingItem] = useState<{ id: string; action: "approve" | "reject" } | undefined>()

  const { data: users, refetch } = useGetUserUnregistrationProposals(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount = users?.totalItems && users.totalItems > 0 ? Math.ceil(users.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const { mutate } = useTransactMutationWithStatus()

  const columns = useMemo(
    () => [
      { accessor: "user", label: t("columns:user"), type: "user", className: "w-[22%]" },
      { accessor: "createdBy", label: t("columns:createdBy"), type: "created-by", className: "w-[14%]" },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.proposalTitle"),
        type: "progress",
        className: "w-[14%]",
      },
      { accessor: "yourVote", label: t("columns:yourVote"), type: "your-vote", className: "w-[16%]" },
      { accessor: "lastsFor", label: t("columns:openFor"), type: "lasts-for", className: "w-[12%]" },
      ...(approval
        ? [
            {
              accessor: "actions",
              label: t("columns:actions"),
              type: "actions",
              className: "w-[20%] text-center",
            },
          ]
        : []),
    ],
    [t, approval],
  )

  const vote = useCallback(
    (id: string, name: string, action: "approve" | "reject") => {
      setLoadingItem({ id: id, action })

      const operation = new ProposalVoting(id, approval!.id, action === "approve" ? 0 : -1)
      mutate(operation, {
        onSuccess: () => {
          invalidateOperation("user-unregistration", { storeId: storeId! })
          const message =
            action === "approve"
              ? t("toast:userUnregistrationApproved", { name })
              : t("toast:userUnregistrationRejected", { name })
          showToast(message, "success")
        },
        onError: err => {
          showToast(err.toString(), "error")
        },
        onSettled: () => {
          setLoadingItem(undefined)
          refetch()
        },
      })
    },
    [approval, mutate, invalidateOperation, storeId, t, refetch],
  )

  const handleApprove = useCallback((id: string, name: string) => vote(id, name, "approve"), [vote])

  const handleReject = useCallback((id: string, name: string) => vote(id, name, "reject"), [vote])

  const votesRequired = useMemo(
    () => calculateVotesRequiredToWinProposal("user-unregistration", store, policies),
    [policies, store],
  )

  const itemRenderer = useMemo(
    () => getItemRenderer(t, handleApprove, handleReject, votesRequired, loadingItem, approval?.id, user?.id),
    [approval, handleApprove, handleReject, loadingItem, t, user?.id, votesRequired],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  return (
    <>
      <Table
        columns={columns}
        items={users?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noUserUnregistrations")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </>
  )
}
