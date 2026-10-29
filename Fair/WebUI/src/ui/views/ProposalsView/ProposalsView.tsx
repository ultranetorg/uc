import { memo, useMemo } from "react"
import { TFunction } from "i18next"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { SvgSearchMd, SvgX } from "assets"
import { Proposal, TotalItemsResult } from "types"
import { Input, Pagination, Table, TableColumn, TableEmptyState } from "ui/components"
import { ProposalType } from "types/ProposalType"

import { getProposalsItemRenderer } from "./proposalsItemRenderer"

const getVotingColumns = (t: TFunction): TableColumn[] => [
  { accessor: "leadingOption", label: t("columns:leadingOption"), type: "leading-option", className: "w-[16%]" },
  {
    accessor: "progress",
    label: t("columns:progress.label"),
    title: t("columns:progress.proposalTitle"),
    type: "progress",
    className: "w-[13%]",
  },
  { accessor: "yourVote", label: t("columns:yourVote"), type: "your-vote", className: "w-[12%]" },
  { accessor: "lastsFor", label: t("columns:openFor"), type: "lasts-for", className: "w-[8%]" },
]

export type ProposalsViewProps = {
  t: TFunction
  proposalType: ProposalType
  proposals?: TotalItemsResult<Proposal>
  page: number
  pagesCount: number
  search: string
  onPageChange: (page: number) => void
  onTableRowClick: (id: string) => void
  onSearchChange: (search: string) => void
  onSearchCancel: () => void
}

export const ProposalsView = memo(
  ({
    t,
    proposalType,
    proposals,
    page,
    pagesCount,
    search,
    onPageChange,
    onTableRowClick,
    onSearchChange,
    onSearchCancel,
  }: ProposalsViewProps) => {
    const columns = useMemo(
      () => [
        { accessor: "text", label: t("columns:title"), type: "title", className: "w-[22%]" },
        { accessor: "createdBy", label: t("columns:createdBy"), type: "account", className: "w-[13%]" },
        { accessor: "action", label: t("columns:action"), type: "action", className: "w-[16%]" },
        ...getVotingColumns(t),
      ],
      [t],
    )

    const { store } = useStoreContext()
    const policies = useStorePoliciesContext()
    const { isModerator, publisherIds } = useStoreRolesContext()
    const { user } = useUserContext()

    const moderatorId = isModerator ? user?.id : undefined
    const itemRenderer = useMemo(
      () => getProposalsItemRenderer(t, { store, policies, moderatorId, publisherIds }),
      [t, store, policies, moderatorId, publisherIds],
    )

    return (
      <>
        <div className="flex flex-col justify-between gap-4 xl:flex-row">
          <div className="w-full max-w-120">
            <Input
              placeholder={
                proposalType === "proposal" ? t("placeholders:searchProposals") : t("placeholders:searchReferendums")
              }
              value={search}
              onChange={onSearchChange}
              elementAfter={
                <>
                  {search && (
                    <div onClick={onSearchCancel} className="cursor-pointer">
                      <SvgX className="stroke-gray-400 hover:stroke-gray-950" />
                    </div>
                  )}
                  <SvgSearchMd className="size-5 shrink-0 stroke-gray-500" />
                </>
              }
            />
          </div>
          <Pagination onPageChange={onPageChange} page={page} pagesCount={pagesCount} />
        </div>
        <Table
          columns={columns}
          items={proposals?.items}
          itemRenderer={itemRenderer}
          emptyState={
            <TableEmptyState
              className="first-letter:uppercase"
              message={!search ? t("noProposals") : t("common:noResults")}
            />
          }
          onRowClick={onTableRowClick}
        />
        <div className="flex justify-end">
          <Pagination onPageChange={onPageChange} page={page} pagesCount={pagesCount} />
        </div>
      </>
    )
  },
)
