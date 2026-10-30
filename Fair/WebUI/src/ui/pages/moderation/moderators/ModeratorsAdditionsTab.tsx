import { useNavigate } from "react-router-dom"
import { ReactNode, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetModeratorProposals } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { ModeratorProposal, Policy, Store } from "types"
import { Badge, Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderCommon, renderCreator, renderProposalProgress, renderUsersList, renderYourVote } from "ui/renderers"
import { calculateVotesRequiredToWinProposal, getVoterIds, parseInteger, routes } from "utils"

type ModeratorsProposalsVoting = {
  store?: Store
  policies?: Policy[]
  currentUserId?: string
  moderatorId?: string
  publisherIds?: string[]
}

const getItemRenderer =
  (t: TFunction, { store, policies, currentUserId, moderatorId, publisherIds }: ModeratorsProposalsVoting = {}) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as ModeratorProposal

    switch (column.type) {
      case "account":
        return renderCreator(proposal.creator)

      case "accounts-list":
        return (
          <div className="flex items-center gap-2">
            {renderUsersList(proposal.moderators)}
            {proposal.moderators.some(x => x.id === currentUserId) && <Badge>{t("common:you")}</Badge>}
          </div>
        )

      case "progress":
        return renderProposalProgress(
          t,
          proposal,
          calculateVotesRequiredToWinProposal(proposal.operation, store, policies),
        )

      case "your-vote":
        return renderYourVote(t, proposal, getVoterIds(proposal.operation, policies, moderatorId, publisherIds))
    }

    return renderCommon(t, column, proposal)
  }

export const ModeratorsAdditionsTab = () => {
  const navigate = useNavigate()
  const { t } = useTranslation("moderatorsAdditionsTab")

  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { isModerator, publisherIds } = useStoreRolesContext()
  const { user } = useUserContext()

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
        label: t("columns:moderators"),
        type: "accounts-list",
        className: "w-[36%]",
      },
      { accessor: "by", label: t("columns:createdBy"), type: "account", className: "w-[16%]" },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.proposalTitle"),
        type: "progress",
        className: "w-[18%]",
      },
      { accessor: "yourVote", label: t("columns:yourVote"), type: "your-vote", className: "w-[16%]" },
      { accessor: "lastsFor", label: t("columns:openFor"), type: "lasts-for", className: "w-[14%]" },
    ],
    [t],
  )

  const currentUserId = user?.id
  const moderatorId = isModerator ? currentUserId : undefined
  const itemRenderer = useMemo(
    () => getItemRenderer(t, { store, policies, currentUserId, moderatorId, publisherIds }),
    [t, store, policies, currentUserId, moderatorId, publisherIds],
  )

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
