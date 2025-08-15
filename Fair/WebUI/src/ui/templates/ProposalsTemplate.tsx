import { memo, useMemo } from "react"
import { TFunction } from "i18next"

import { Proposal, TotalItemsResult } from "types"
import { Input, Pagination, Table, TableEmptyState, TableRowRenderer } from "ui/components"
import { getProposalsItemRenderer } from "ui/renderers"
import { getCommonColumns } from "ui/pages/Moderation/Moderation/constants"

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
        { accessor: "action", label: t("common:action"), type: "action", className: "w-[6%]" },
        ...getCommonColumns(t),
      ],
      [t],
    )

    const itemRenderer = useMemo(() => getProposalsItemRenderer(t), [t])

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
          itemRenderer={itemRenderer}
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
