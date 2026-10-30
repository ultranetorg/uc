import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetUserRegistrationProposals, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useApprovalIdentityForOperation, useResolveStoreId, useUrlParamsState } from "hooks"
import { ProposalVoting } from "types"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { getUsersItemRenderer } from "ui/renderers"
import { calculateVotesRequiredToWinProposal, parseInteger, showToast } from "utils"

export const NewUsersTab = () => {
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
    () => getUsersItemRenderer(t, handleApprove, handleReject, votesRequired, loadingItem, approval?.id, user?.id),
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
