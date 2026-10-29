import { useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetChangedPublications } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { changedPublicationItemRenderer } from "ui/renderers"
import { parseInteger, routes } from "utils"

export const ChangedPublicationsTab = () => {
  const { t } = useTranslation("tabChangedPublications")
  const navigate = useNavigate()
  const storeId = useResolveStoreId()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: publications } = useGetChangedPublications(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
      : 0

  const columns = useMemo(
    () => [
      {
        accessor: "publication",
        label: t("columns:product"),
        type: "publication",
        className: "w-[18%]",
      },
      { accessor: "author", label: t("columns:author"), type: "account", className: "w-[15%]" },
      {
        accessor: "category",
        label: t("columns:category"),
        type: "category",
        className: "w-[13%]",
      },
      {
        accessor: "currentVersion",
        label: t("columns:currentVersion"),
        type: "version",
        className: "w-[10%] text-right",
      },
      {
        accessor: "latestVersion",
        label: t("columns:latestVersion"),
        type: "version",
        className: "w-[10%] text-right",
      },
    ],
    [t],
  )

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.moderation.changedPublication(storeId!, id)),
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
        itemRenderer={changedPublicationItemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("noPublications")} />}
        onRowClick={handleTableRowClick}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
