import { useCallback, useEffect, useMemo } from "react"
import { Navigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { AUCTIONS_ITEMS_PER_PAGE_DEFAULT_VALUE, AUCTIONS_ITEMS_PER_PAGE_OPTIONS, Breakpoints } from "constants"
import { useGetAuctions } from "entities"
import { useMediaQuery, useUrlPagination, usePageId } from "hooks"
import { Table, TableItem, PageLoader, AuctionsTableFooter } from "ui/components"

import { getTableColumns } from "./constants"
import { getTableItemRenderer } from "./tableItemRenderer"

export const AuctionsPage = () => {
  useDocumentTitle("Auctions | Ultranet Explorer")

  const pageId = usePageId()
  const { t } = useTranslation(pageId)
  const { currency } = useSettings()
  const { page, pageSize, setPage, setPageSize } = useUrlPagination()
  const isMediumView = useMediaQuery(Breakpoints.md)
  const { data: auctions, isLoading } = useGetAuctions({ page, pageSize })

  const tableItemRenderer = useMemo(
    () => getTableItemRenderer(currency, auctions?.rateUsd, auctions?.emissionMultiplier),
    [auctions?.emissionMultiplier, auctions?.rateUsd, currency],
  )

  const columns = useMemo(() => getTableColumns(t, !isMediumView), [t, isMediumView])

  const items: TableItem[] | undefined = useMemo(
    () =>
      auctions && auctions.items.length > 0 ? auctions.items.map(item => ({ ...item, id: item.name })) : undefined,
    [auctions],
  )

  const handlePageChange = useCallback((page: number) => setPage(page), [setPage])

  const handlePageSizeChange = useCallback((pageSize: number) => setPageSize(pageSize), [setPageSize])

  useEffect(() => {
    if (!auctions) {
      return
    }

    if (!AUCTIONS_ITEMS_PER_PAGE_OPTIONS.includes(pageSize)) {
      setPageSize(AUCTIONS_ITEMS_PER_PAGE_DEFAULT_VALUE)
    }

    const pagesCount = Math.round(auctions.totalItems / pageSize)
    if (page >= pagesCount) {
      setPage(0)
    }
  }, [auctions, page, pageSize, setPage, setPageSize])

  if (isLoading) {
    return <PageLoader />
  }

  if (!auctions) {
    return <Navigate to="/" />
  }

  return (
    <div>
      <div className="mb-5 mt-11 font-sans-medium text-2xl leading-6 text-white">{t("pageTitle")}</div>
      <Table
        itemRenderer={tableItemRenderer}
        columns={columns}
        items={items ?? []}
        footer={
          <AuctionsTableFooter
            page={page}
            pageSize={pageSize || AUCTIONS_ITEMS_PER_PAGE_DEFAULT_VALUE}
            totalItems={auctions.totalItems}
            itemsPerPage={pageSize || AUCTIONS_ITEMS_PER_PAGE_DEFAULT_VALUE}
            itemsPerPageValues={AUCTIONS_ITEMS_PER_PAGE_OPTIONS}
            onPageChange={handlePageChange}
            onItemsPerPageChange={handlePageSizeChange}
          />
        }
      />
    </div>
  )
}
