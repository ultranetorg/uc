import { memo, ReactNode, useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useCanVoteOperation, useResolveStoreId, useUrlParamsState } from "hooks"
import { OperationType, Policy, PublicationProposal, Store, TotalItemsResult } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import {
  renderAuthor2,
  renderCreator,
  renderOpenFor,
  renderProposalProgress,
  renderPublication,
  renderTitle,
  renderWithYouBadge,
  renderYourVote,
} from "ui/renderers"
import { calculateVotesRequiredToWinProposal, getVoterIds, parseInteger, routes } from "utils"

type PublicationsVoting = {
  store?: Store
  policies?: Policy[]
  moderatorId?: string
  publisherIds?: string[]
}

const getItemRenderer =
  (t: TFunction, { store, policies, moderatorId, publisherIds }: PublicationsVoting = {}) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as PublicationProposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "publication":
        return renderPublication(proposal.publication)
      case "author":
        return renderWithYouBadge(
          t,
          renderAuthor2(proposal.authorTitle, proposal.authorLogoId),
          !!publisherIds?.includes(proposal.authorId),
        )
      case "progress":
        return renderProposalProgress(
          t,
          proposal,
          calculateVotesRequiredToWinProposal(proposal.operation, store, policies),
        )
      case "your-vote":
        return renderYourVote(t, proposal, getVoterIds(proposal.operation, policies, moderatorId, publisherIds))
      case "account":
        return renderCreator(proposal.creator)

      case "open-for":
        return renderOpenFor(t, proposal.creationTime)
    }

    return undefined
  }

export type PublicationsOperationTabProps = {
  operation: OperationType
  useGetProposals: (
    storeId?: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ) => { data?: TotalItemsResult<PublicationProposal> }
}

export const PublicationsOperationTab = memo(({ operation, useGetProposals }: PublicationsOperationTabProps) => {
  const { t } = useTranslation("publicationsOperationTab")
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

  const { data: publications } = useGetProposals(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
      : 0

  const columns = useMemo(
    () => [
      { accessor: "publication", label: t("columns:product"), type: "publication", className: "w-[28%]" },
      { accessor: "author", label: t("columns:author"), type: "author", className: "w-[16%]" },
      { accessor: "by", label: t("columns:createdBy"), type: "account", className: "w-[14%]" },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.proposalTitle"),
        type: "progress",
        className: "w-[14%]",
      },
      ...(canVote
        ? [{ accessor: "yourVote", label: t("columns:yourVote"), type: "your-vote", className: "w-[14%] text-center" }]
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
    (id: string) => navigate(routes.moderation.moderatorPublication(storeId!, id)),
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
        items={publications?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noPublications")} />}
        onRowClick={handleTableRowClick}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
})
