import { memo, useMemo } from "react"
import { TFunction } from "i18next"

import { SvgSearchMd, SvgX } from "assets"
import { Proposal, TotalItemsResult } from "types"
import { Input, Pagination, Table, TableEmptyState } from "ui/components"
import { getVotingColumns } from "ui/pages/moderation/constants"
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
  onSearchCancel: () => void
}

export const ProposalsView = memo(
  ({
    t,
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
        { accessor: "text", label: t("common:title"), type: "title", className: "w-[24%]" },
        { accessor: "createdBy", label: t("common:createdBy"), type: "account", className: "w-[15%]" },
        { accessor: "action", label: t("common:action"), type: "action", className: "w-[20%]" },
        ...getVotingColumns(t),
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
              iconAfter={
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
