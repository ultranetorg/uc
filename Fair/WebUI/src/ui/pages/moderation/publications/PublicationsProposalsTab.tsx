import { ReactNode, useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublicationProposals } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { Policy, PublicationProposal, Store } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import {
  renderActionShort,
  renderAuthor2,
  renderCommon,
  renderCreator,
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
      case "action-short":
        return renderActionShort(t, proposal.operation)
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
    }

    return renderCommon(t, column, proposal)
  }

export const PublicationsProposalsTab = () => {
  const { t } = useTranslation("publicationsProposalsTab")
  const navigate = useNavigate()

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

  const { data: publications } = useGetPublicationProposals(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
      : 0

  const columns = useMemo(
    () => [
      { accessor: "publication", label: t("columns:product"), type: "publication", className: "w-[25%]" },
      { accessor: "author", label: t("columns:author"), type: "author", className: "w-[14%]" },
      { accessor: "action", label: t("columns:action"), type: "action-short", className: "w-[11%]" },
      { accessor: "by", label: t("columns:createdBy"), type: "account", className: "w-[12%]" },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.proposalTitle"),
        type: "progress",
        className: "w-[13%]",
      },
      { accessor: "yourVote", label: t("columns:yourVote"), type: "your-vote", className: "w-[12%]" },
      { accessor: "lastsFor", label: t("columns:openFor"), type: "lasts-for", className: "w-[13%]" },
    ],
    [t],
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
}
