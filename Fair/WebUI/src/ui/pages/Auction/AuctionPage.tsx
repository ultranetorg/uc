import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Navigate, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { Breakpoints, DEFAULT_PAGE_SIZE } from "constants"
import { useGetAuction, useGetAuctionBids } from "entities"
import { usePageId, useMediaQuery, useBreakpoint } from "hooks"
import { PageLoader, Table, TableColumn, TableItem, List, ListRow, TableFooter } from "ui/components"
import { getListItemRenderer, getTableItemRenderer } from "ui/renderers"

import { getListRows, getTableColumns } from "./constants"

export const AuctionPage = () => {
  const { auctionId } = useParams()
  useDocumentTitle(auctionId ? `Auction - ${auctionId} | Ultranet Explorer` : "Auction | Ultranet Explorer")

  const pageId = usePageId()
  const { t } = useTranslation(pageId)
  const isLargeView = useMediaQuery(Breakpoints.lg)
  const { currency } = useSettings()
  const breakpoint = useBreakpoint()

  const [page, setPage] = useState(0)

  const { data: auction, isLoading } = useGetAuction(auctionId) // TODO: check auctionId is present.
  const { data: bids, isLoading: isBidsLoading } = useGetAuctionBids(auction?.name, page)

  const [items, setItems] = useState<TableItem[] | undefined>()

  const rows: ListRow[] = useMemo(() => getListRows(t), [t])

  const columns: TableColumn[] = useMemo(() => getTableColumns(t, !isLargeView), [isLargeView, t])

  const listItemRenderer = useMemo(
    () => getListItemRenderer(currency, auction?.rateUsd, auction?.emissionMultiplier),
    [auction?.rateUsd, auction?.emissionMultiplier, currency],
  )

  const tableItemRenderer = useMemo(
    () => getTableItemRenderer(pageId, breakpoint, currency, auction?.rateUsd, auction?.emissionMultiplier),
    [pageId, breakpoint, currency, auction?.rateUsd, auction?.emissionMultiplier],
  )

  useEffect(() => {
    if (isLoading || isBidsLoading || auction!.bids.totalItems === 0) {
      setItems([])
      return
    }

    const bidsItems = page > 0 && bids ? bids.items : auction?.bids.items
    const items = bidsItems ? bidsItems.map(item => ({ ...item, id: item.bidBy + item.bidDay })) : undefined
    setItems(items)
  }, [auction, bids, isBidsLoading, isLoading, page])

  if (isLoading) {
    return <PageLoader />
  }

  if (!auction) {
    return <Navigate to={"/"} />
  }

  return (
    <div className="flex flex-col gap-7 leading-[1.125rem]">
      <List items={{ id: auction.name, ...auction }} rows={rows} itemRenderer={listItemRenderer} />
      {items && !!items.length && (
        <Table
          title={t("bids")}
          columns={columns}
          items={items}
          itemRenderer={tableItemRenderer}
          footer={
            <TableFooter
              pageSize={DEFAULT_PAGE_SIZE}
              page={page}
              onPageChange={setPage}
              totalItems={auction.bids.totalItems}
            />
          }
        />
      )}
    </div>
  )
}
