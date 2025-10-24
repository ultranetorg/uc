import { memo, useMemo } from "react"
import { TFunction } from "i18next"

import { Proposal, TotalItemsResult } from "types"
import { Input, Pagination, Table, TableEmptyState } from "ui/components"
import { getCommonColumns } from "ui/pages/Moderation/Moderation/constants"
import { getProposalsItemRenderer } from "ui/renderers"

export type ProposalsViewProps = {
  t: TFunction
  proposals?: TotalItemsResult<Proposal>
  page: number
  pagesCount: number
  search: string
  onPageChange: (page: number) => void
  onTableRowClick: (id: string) => void
  onSearchChange: (search: string) => void
}

export const ProposalsView = memo(
  ({ t, proposals, page, pagesCount, search, onPageChange, onTableRowClick, onSearchChange }: ProposalsViewProps) => {
    const columns = useMemo(
      () => [
        { accessor: "text", label: t("common:title"), type: "title", className: "w-[24%]" },
        { accessor: "createdBy", label: t("common:createdBy"), type: "account", className: "w-[15%]" },
        { accessor: "action", label: t("common:action"), type: "action", className: "w-[20%]" },
        ...getCommonColumns(t),
      ],
      [t],
    )

    const itemRenderer = useMemo(() => getProposalsItemRenderer(t), [t])

    return (
      <>
        <div className="flex flex-col justify-between gap-4 xl:flex-row">
          <div className="w-full max-w-120">
            <Input
              placeholder={t("searchProposal")}
              value={search}
              onChange={onSearchChange}
              id="referendums-search-input"
            />
          </div>
          <Pagination onPageChange={onPageChange} page={page} pagesCount={pagesCount} />
        </div>
        <Table
          columns={columns}
          items={proposals?.items}
          itemRenderer={itemRenderer}
          emptyState={<TableEmptyState message={t("noProposals")} />}
          onRowClick={onTableRowClick}
        />
        <div className="flex justify-end">
          <Pagination onPageChange={onPageChange} page={page} pagesCount={pagesCount} />
        </div>
      </>
    )
  },
)
