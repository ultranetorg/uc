import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useOperationPolicy, useSiteContext, useSitePoliciesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetUserRegistrationProposals } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useParams, useUrlParamsState } from "hooks"
import { ProposalVoting } from "types"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { getUsersItemRenderer } from "ui/renderers"
import { calculateVotesRequiredToWinProposal, parseInteger, showToast } from "utils"

export const NewUsersTab = () => {
  const { siteId } = useParams()
  const { voterId } = useOperationPolicy("user-registration")
  const { site } = useSiteContext()
  const { policies } = useSitePoliciesContext()
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

  const { data: users, refetch } = useGetUserRegistrationProposals(siteId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount = users?.totalItems && users.totalItems > 0 ? Math.ceil(users.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const { mutate } = useTransactMutationWithStatus()

  const columns = useMemo(
    () => [
      { accessor: "signer", label: t("common:user"), type: "account", className: "w-[40%]" },
      { accessor: "lastsFor", label: t("common:lastsFor"), type: "lasts-for", className: "w-[15%]" },
      {
        accessor: "votes",
        label: t("common:votes"),
        type: "votes",
        className: "first-letter:uppercase w-[15%] text-center",
      },
      ...(voterId
        ? [
            {
              accessor: "actions",
              label: t("common:actions"),
              type: "actions",
              className: "w-[20%] text-center first-letter:uppercase",
            },
          ]
        : []),
    ],
    [t, voterId],
  )

  const vote = useCallback(
    (id: string, name: string, action: "approve" | "reject") => {
      setLoadingItem({ id: id, action })

      const operation = new ProposalVoting(id, voterId!, action === "approve" ? 0 : -1)
      mutate(operation, {
        onSuccess: () => {
          const message =
            action === "approve"
              ? t("toast:userRegistrationApproved", { name })
              : t("toast:userRegistrationRejected", { name })
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
    [mutate, refetch, t, voterId],
  )

  const handleApprove = useCallback((id: string, name: string) => vote(id, name, "approve"), [vote])

  const handleReject = useCallback((id: string, name: string) => vote(id, name, "reject"), [vote])

  const votesRequired = useMemo(
    () => calculateVotesRequiredToWinProposal("user-registration", site, policies),
    [policies, site],
  )

  const itemRenderer = useMemo(
    () => getUsersItemRenderer(t, handleApprove, handleReject, votesRequired, loadingItem, voterId),
    [handleApprove, handleReject, loadingItem, t, voterId, votesRequired],
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
