import { memo, useMemo } from "react"
import { TFunction } from "i18next"

import { Proposal, TotalItemsResult } from "types"
import { Input, Pagination, Table, TableEmptyState, TableRowRenderer } from "ui/components"
import { proposalsItemRenderer } from "ui/renderers"

export type ProposalsTemplateProps = {
  t: TFunction
  proposals?: TotalItemsResult<Proposal>
  page: number
  pagesCount: number
  search: string
  tableRowRenderer: TableRowRenderer
  onSearchChange: (search: string) => void
  onPageChange: (page: number) => void
}

export const ProposalsTemplate = memo(
  ({
    t,
    proposals,
    page,
    pagesCount,
    search,
    tableRowRenderer,
    onSearchChange,
    onPageChange,
  }: ProposalsTemplateProps) => {
    const columns = useMemo(
      () => [
        { accessor: "text", label: t("common:title"), type: "title", className: "w-[24%]" },
        { accessor: "createdBy", label: t("common:createdBy"), type: "account", className: "w-[15%]" },
        { accessor: "expirationTime", label: t("common:daysLeft"), className: "w-[6%] text-center" },
        {
          accessor: "optionsVotesCount",
          label: t("common:options"),
          type: "options",
          className: "w-[16%] text-center",
        },
        { accessor: "abstainedCount", label: t("common:abs"), className: "w-[5%] text-center" },
        { accessor: "neitherCount", label: t("common:neither"), className: "w-[5%] text-center" },
        { accessor: "banCount", label: t("common:ban"), className: "w-[5%] text-center" },
        { accessor: "banishCount", label: t("common:banish"), className: "w-[5%] text-center" },
      ],
      [t],
    )

    return (
      <>
        <div className="flex flex-col justify-between gap-4 xl:flex-row">
          <Input
            placeholder={t("searchProposal")}
            value={search}
            onChange={onSearchChange}
            id="referendums-search-input"
            className="w-full max-w-120"
          />
          <Pagination onPageChange={onPageChange} page={page} pagesCount={pagesCount} />
        </div>
        <Table
          columns={columns}
          items={proposals?.items}
          itemRenderer={proposalsItemRenderer}
          rowRenderer={tableRowRenderer}
          emptyState={<TableEmptyState message={t("noProposals")} />}
        />
        <div className="flex justify-end">
          <Pagination onPageChange={onPageChange} page={page} pagesCount={pagesCount} />
        </div>
      </>
    )
  },
)
