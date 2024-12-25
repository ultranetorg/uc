import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Navigate, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { DEFAULT_PAGE_SIZE } from "constants"
import { useGetTransaction, useGetTransactionOperations } from "entities"
import { useGetOperationDesc, usePageId } from "hooks"
import { List, ListRow, OperationsTable, PageLoader, TableItem } from "ui/components"
import { getListItemRenderer } from "ui/renderers"

import { getListRows } from "./constants"

export const TransactionPage = () => {
  const { transactionId } = useParams()
  useDocumentTitle(
    transactionId ? `Transaction - ${transactionId} | Ultranet Explorer` : "Transaction | Ultranet Explorer",
  )

  const pageId = usePageId()
  const { t } = useTranslation(pageId)
  const { currency } = useSettings()
  const { getOperationDesc } = useGetOperationDesc()

  const [page, setPage] = useState(0)

  const { data: transaction, isLoading } = useGetTransaction(transactionId)
  const { data: operations, isLoading: isOperationsLoading } = useGetTransactionOperations(
    transaction?.id,
    page,
    DEFAULT_PAGE_SIZE,
  )

  const [items, setItems] = useState<TableItem[] | undefined>()

  const rows: ListRow[] = useMemo(() => getListRows(t), [t])

  const listItemRenderer = useMemo(
    () => getListItemRenderer(currency, transaction?.rateUsd, transaction?.emissionMultiplier),
    [currency, transaction?.emissionMultiplier, transaction?.rateUsd],
  )

  useEffect(() => {
    if (isLoading || isOperationsLoading || transaction!.operations.totalItems === 0) {
      setItems([])
      return
    }

    const info = page > 0 && operations ? operations.items : transaction!.operations.items
    const items = info
      ? info.map(item => ({
          ...item,
          id: item.id.toString(),
          description: getOperationDesc(item, currency, transaction!.rateUsd, transaction!.emissionMultiplier),
        }))
      : undefined
    setItems(items)
  }, [currency, isLoading, isOperationsLoading, operations, page, transaction, getOperationDesc])

  if (isLoading) {
    return <PageLoader />
  }

  if (!transaction) {
    return <Navigate to={"/"} />
  }

  return (
    <div className="flex flex-col gap-7 leading-[1.125rem]">
      <List items={transaction} rows={rows} itemRenderer={listItemRenderer} />
      {items && !!items.length && (
        <OperationsTable
          title={t("operations")}
          totalItems={transaction.operations.totalItems}
          items={items}
          page={page}
          onPageChange={setPage}
          currency={currency}
          rateUsd={transaction.rateUsd}
          emissionMultiplier={transaction.emissionMultiplier}
        />
      )}
    </div>
  )
}
