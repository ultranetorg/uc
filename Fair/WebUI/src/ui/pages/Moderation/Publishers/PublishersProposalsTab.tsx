import { useCallback, useMemo, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublisherProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { parseInteger } from "utils"

import { getPublisherProposalsItemRenderer } from "./publisherProposalsItemRenderer"

export const PublishersProposalsTab = () => {
  const { siteId } = useParams()
  const navigate = useNavigate()
  const { t } = useTranslation("publishersProposalsTab")

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: proposals } = useGetPublisherProposals(siteId, "", page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    proposals?.totalItems && proposals.totalItems > 0 ? Math.ceil(proposals.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      {
        accessor: "publishers",
        label: t("common:publishers"),
        type: "accounts-list",
        className: "w-[30%] first-letter:uppercase",
      },
      { accessor: "by", label: t("common:createdBy"), type: "account", className: "w-[12%]" },
      { accessor: "action", label: t("common:action"), type: "action-short", className: "w-[12%]" },
      { accessor: "lastsFor", label: t("common:lastsFor"), type: "lasts-for", className: "w-[12%]" },
      { accessor: "votes", label: t("common:votes"), type: "votes", className: "capitalize w-[12%]" },
      { accessor: "nabb", label: t("common:nabb"), type: "nabb", title: t("common:nabbFull"), className: "w-[12%]" },
    ],
    [t],
  )
  const itemRenderer = useMemo(() => getPublisherProposalsItemRenderer(t), [t])

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/m/a/r/${id}`), [navigate, siteId])

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
