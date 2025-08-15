import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublicationProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { getPublicationsItemRenderer } from "ui/renderers"
import { parseInteger } from "utils"
import { getCommonColumns } from "./constants"

export const PublicationsTab = () => {
  const { t } = useTranslation("tabPublications")
  const { siteId } = useParams()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: publications } = useGetPublicationProposals(siteId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
      : 0

  const columns = useMemo(
    () => [
      { accessor: "title", label: t("common:title"), type: "title", className: "w-[23%]" },
      { accessor: "publication", label: t("common:product"), type: "publication", className: "w-[18%]" },
      { accessor: "author", label: t("common:author"), type: "account", className: "w-[15%]" },
      { accessor: "action", label: t("common:action"), type: "action-short", className: "w-[17%]" },
      ...getCommonColumns(t),
    ],
    [t],
  )
  const itemRenderer = useMemo(() => getPublicationsItemRenderer(t), [t])

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
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
