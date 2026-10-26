import { useNavigate } from "react-router-dom"
import { ReactNode, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetModeratorProposals } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { ModeratorProposal } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderUser, renderUsersList } from "ui/renderers2"
import { renderActionShort, renderCommon } from "ui/renderers/utils"
import { calculateVotesRequiredToWinProposal, parseInteger, routes } from "utils"

const getModeratorsProposalsItemRenderer =
  (t: TFunction, votesRequired?: number) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as ModeratorProposal

    switch (column.type) {
      case "account":
        return renderUser(proposal.by)
      case "accounts-list":
        return renderUsersList(proposal.moderators)
      case "action-short":
        return renderActionShort(t, proposal.operation)
    }

    return renderCommon(t, column, proposal, votesRequired)
  }

export const AddModeratorsTab = () => {
  const navigate = useNavigate()
  const { t } = useTranslation("moderatorsProposalsTab")

  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: proposals } = useGetModeratorProposals(storeId, "", page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    proposals?.totalItems && proposals.totalItems > 0 ? Math.ceil(proposals.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      {
        accessor: "moderators",
        label: t("common:moderators"),
        type: "accounts-list",
        className: "w-[30%] first-letter:uppercase",
      },
      { accessor: "action", label: t("common:action"), type: "action-short", className: "w-[12%]" },
      { accessor: "votes", label: t("common:votes"), type: "votes", className: "capitalize w-[12%] text-center" },
    ],
    [t],
  )

  const votesRequired = useMemo(
    () => calculateVotesRequiredToWinProposal("store-moderator-addition", store, policies),
    [policies, store],
  )

  const itemRenderer = useMemo(() => getModeratorsProposalsItemRenderer(t, votesRequired), [t, votesRequired])

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.moderation.moderatorProposal(storeId!, id)),
    [navigate, storeId],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  return (
    <div className="flex flex-col gap-6">
      <Table
        columns={columns}
        items={proposals?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noProposals")} />}
        onRowClick={handleTableRowClick}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
