import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Navigate, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { Breakpoints, DEFAULT_PAGE_SIZE } from "constants"
import { useGetRound, useGetRoundTransactions } from "entities"
import { useMediaQuery, usePageId } from "hooks"
import { List, ListRow, PageLoader, Table, TableColumn, TableItem, TableFooter } from "ui/components"
import { getListItemRenderer, getTableItemRenderer } from "ui/renderers"

import { getListRows, getTableColumns } from "./constants"

export const RoundPage = () => {
  const { roundId } = useParams()
  useDocumentTitle(roundId ? `Round - ${roundId} | Ultranet Explorer` : "Round | Ultranet Explorer")

  const pageId = usePageId()
  const { t } = useTranslation(pageId)
  const { currency } = useSettings()
  const isSmallView = useMediaQuery(Breakpoints.xs)
  const isMediumView = useMediaQuery(Breakpoints.md)

  const [page, setPage] = useState(0)

  const { data: round, isLoading } = useGetRound(roundId) // TODO: check accountId is present.
  const { data: transactions, isLoading: isTransactionsLoading } = useGetRoundTransactions(round?.id, page)

  const [items, setItems] = useState<TableItem[] | undefined>()

  const rows: ListRow[] = useMemo(() => getListRows(t), [t])

  const tableItemRenderer = useMemo(() => getTableItemRenderer("transactions"), [])

  const listItemRenderer = useMemo(
    () => getListItemRenderer(currency, round?.rateUsd, round?.emissionMultiplier),
    [currency, round?.rateUsd, round?.emissionMultiplier],
  )

  const columns: TableColumn[] = useMemo(
    () => getTableColumns(t, isMediumView, isSmallView),
    [isMediumView, isSmallView, t],
  )

  useEffect(() => {
    if (isLoading || isTransactionsLoading || round!.transactions.totalItems === 0) {
      setItems([])
      return
    }

    const transactionsItems = page > 0 && transactions ? transactions.items : round!.transactions.items
    const items = transactionsItems ? transactionsItems.map(item => ({ ...item, id: item.id.toString() })) : undefined
    setItems(items)
  }, [isLoading, isTransactionsLoading, page, round, transactions])

  if (isLoading) {
    return <PageLoader />
  }

  if (!round) {
    return <Navigate to={"/"} />
  }

  return (
    <div className="flex flex-col gap-7 leading-[1.125rem]">
      <List items={round} rows={rows} itemRenderer={listItemRenderer} />
      {items && !!items.length && (
        <Table
          title={t("transactions")}
          itemRenderer={tableItemRenderer}
          columns={columns}
          items={items}
          footer={
            <TableFooter
              pageSize={DEFAULT_PAGE_SIZE}
              page={page}
              onPageChange={setPage}
              totalItems={round.transactions.totalItems}
            />
          }
        />
      )}
    </div>
  )
}
