import { useCallback, useMemo, useState, ReactNode } from "react"
import { TFunction } from "i18next"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetUserRegistrationProposals, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useApprovalIdentityForOperation, useResolveStoreId, useUrlParamsState } from "hooks"
import { Proposal, ProposalVoting } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import {
  renderActions,
  renderCommon,
  renderProposalProgress,
  renderTitle,
  renderWithYouBadge,
  renderYourVote,
} from "ui/renderers/utils"
import { renderCreator } from "ui/renderers2"
import { calculateVotesRequiredToWinProposal, isUserVoted, parseInteger, showToast } from "utils"

const getItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string, name: string) => void,
    onReject: (id: string, name: string) => void,
    votesRequired?: number,
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
    currentUserId?: string,
    userId?: string,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as Proposal
    const isVoted = isUserVoted(currentUserId, proposal)

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)

      case "account":
        return renderWithYouBadge(t, renderCreator(proposal.creator), proposal.creator.id === userId)

      case "progress":
        return renderProposalProgress(t, proposal, votesRequired ?? 0)

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

export const UsersNewTab = () => {
  const storeId = useResolveStoreId()
  const approval = useApprovalIdentityForOperation("user-registration")
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

  const { data: users, refetch } = useGetUserRegistrationProposals(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount = users?.totalItems && users.totalItems > 0 ? Math.ceil(users.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const { mutate } = useTransactMutationWithStatus()

  const columns = useMemo(
    () => [
      { accessor: "signer", label: t("columns:user"), type: "account", className: "w-[28%]" },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.proposalTitle"),
        type: "progress",
        className: "w-[16%]",
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
          const message =
            action === "approve"
              ? t("toast:userRegistrationApproved", { name })
              : t("toast:userRegistrationRejected", { name })
          showToast(message, "success")
          invalidateOperation("user-registration", { storeId: storeId! })
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
    [approval, mutate, t, invalidateOperation, storeId, refetch],
  )

  const handleApprove = useCallback((id: string, name: string) => vote(id, name, "approve"), [vote])

  const handleReject = useCallback((id: string, name: string) => vote(id, name, "reject"), [vote])

  const votesRequired = useMemo(
    () => calculateVotesRequiredToWinProposal("user-registration", store, policies),
    [policies, store],
  )

  const itemRenderer = useMemo(
    () => getItemRenderer(t, handleApprove, handleReject, votesRequired, loadingItem, approval?.id, user?.id),
    [approval?.id, handleApprove, handleReject, loadingItem, t, user?.id, votesRequired],
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
        emptyState={<TableEmptyState message={t("noUserRegistrations")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </>
  )
}
