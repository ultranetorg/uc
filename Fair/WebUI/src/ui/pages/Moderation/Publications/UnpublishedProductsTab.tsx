import { useCallback, useMemo, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetUnpublishedSiteProducts } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { unpublishedProductItemRenderer } from "ui/renderers"
import { parseInteger } from "utils"

export const UnpublishedProductsTab = () => {
  const { t } = useTranslation("tabUnpublishedProducts")
  const navigate = useNavigate()
  const { siteId } = useParams()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: products } = useGetUnpublishedSiteProducts(siteId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    products?.totalItems && products.totalItems > 0 ? Math.ceil(products.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      { accessor: "publication", label: t("common:product"), type: "publication", className: "w-[40%]" },
      { accessor: "author", label: t("common:author"), type: "account", className: "w-[60%]" },
    ],
    [t],
  )

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/m/n/${id}`), [navigate, siteId])

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
        items={products?.items}
        itemRenderer={unpublishedProductItemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("noProducts")} />}
        onRowClick={handleTableRowClick}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
