import { memo, ReactNode, useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useCanVoteOperation, useResolveStoreId, useUrlParamsState } from "hooks"
import { OperationType, Policy, Proposal, Store, TotalItemsResult } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderCreator, renderOpenFor, renderProposalProgress, renderTitle, renderYourVote } from "ui/renderers"
import { calculateVotesRequiredToWinProposal, getVoterIds, parseInteger } from "utils"

type ProposalsVoting = {
  store?: Store
  policies?: Policy[]
  moderatorId?: string
  publisherIds?: string[]
}

const getItemRenderer =
  (t: TFunction, { store, policies, moderatorId, publisherIds }: ProposalsVoting = {}) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as Proposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "account":
        return renderCreator(proposal.creator)
      case "progress":
        return renderProposalProgress(
          t,
          proposal,
          calculateVotesRequiredToWinProposal(proposal.operation, store, policies),
        )
      case "your-vote":
        return renderYourVote(t, proposal, getVoterIds(proposal.operation, policies, moderatorId, publisherIds))
      case "open-for":
        return renderOpenFor(t, proposal.creationTime)
    }

    return undefined
  }

export type ProposalsOperationTabProps = {
  operation: OperationType
  useGetProposals: (
    storeId?: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ) => { data?: TotalItemsResult<Proposal> }
  getProposalPath: (storeId: string, proposalId: string) => string
}

export const ProposalsOperationTab = memo(
  ({ operation, useGetProposals, getProposalPath }: ProposalsOperationTabProps) => {
    const { t } = useTranslation("proposalsOperationTab")
    const navigate = useNavigate()

    const canVoteOperation = useCanVoteOperation()
    const storeId = useResolveStoreId()
    const { store } = useStoreContext()
    const policies = useStorePoliciesContext()
    const { isModerator, publisherIds } = useStoreRolesContext()
    const { user } = useUserContext()

    const canVote = canVoteOperation(operation)

    const [state, setState] = useUrlParamsState({
      page: {
        defaultValue: 0,
        parse: v => parseInteger(v),
        validate: v => isNumber(v) && v >= 0,
      },
    })

    const [page, setPage] = useState(state.page)

    const { data: proposals } = useGetProposals(storeId, page, DEFAULT_PAGE_SIZE_20)
    const pagesCount =
      proposals?.totalItems && proposals.totalItems > 0 ? Math.ceil(proposals.totalItems / DEFAULT_PAGE_SIZE_20) : 0

    const columns = useMemo<TableColumn[]>(
      () => [
        { accessor: "title", label: t("columns:title"), type: "title", className: "w-[34%]" },
        { accessor: "by", label: t("columns:createdBy"), type: "account", className: "w-[20%]" },
        {
          accessor: "progress",
          label: t("columns:progress.label"),
          title: t("columns:progress.proposalTitle"),
          type: "progress",
          className: "w-[16%]",
        },
        ...(canVote
          ? [
              {
                accessor: "yourVote",
                label: t("columns:yourVote"),
                type: "your-vote",
                className: "w-[16%] text-center",
              },
            ]
          : []),
        { accessor: "lastsFor", label: t("columns:openFor"), type: "open-for", className: "w-[14%] text-right" },
      ],
      [canVote, t],
    )

    const moderatorId = isModerator ? user?.id : undefined
    const itemRenderer = useMemo(
      () => getItemRenderer(t, { store, policies, moderatorId, publisherIds }),
      [t, store, policies, moderatorId, publisherIds],
    )

    const handleTableRowClick = useCallback(
      (id: string) => navigate(getProposalPath(storeId!, id)),
      [getProposalPath, navigate, storeId],
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
  },
)
