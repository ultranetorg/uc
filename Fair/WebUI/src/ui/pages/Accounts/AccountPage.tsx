import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Navigate, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { DEFAULT_PAGE_SIZE } from "constants"
import { useGetAccount, useGetAccountOperations } from "entities"
import { useGetOperationDesc, usePageId } from "hooks"
import { PageLoader, List, ListRow, OperationsTable, TableItem } from "ui/components"
import { getListItemRenderer } from "ui/renderers"

import { getListRows } from "./constants"

export const AccountPage = () => {
  const { accountId } = useParams()
  useDocumentTitle(accountId ? `Account - ${accountId} | Ultranet Explorer` : "Account | Ultranet Explorer")

  const pageId = usePageId()
  const { t } = useTranslation(pageId)
  const { currency } = useSettings()
  const { getOperationDesc } = useGetOperationDesc()

  const [page, setPage] = useState(0)

  const { data: account, isLoading } = useGetAccount(accountId)
  const { data: operations, isLoading: isOperationsLoading } = useGetAccountOperations(
    account?.address,
    page,
    DEFAULT_PAGE_SIZE,
  )

  const [items, setItems] = useState<TableItem[] | undefined>()

  const rows: ListRow[] = useMemo(() => getListRows(t), [t])

  const listItemRenderer = useMemo(
    () => getListItemRenderer(currency, account?.rateUsd, account?.emissionMultiplier),
    [currency, account?.rateUsd, account?.emissionMultiplier],
  )

  useEffect(() => {
    if (isLoading || isOperationsLoading || account!.operations.totalItems === 0) {
      setItems([])
      return
    }

    const operationItems = page > 0 && operations ? operations.items : account!.operations.items
    const items = operationItems.map(item => ({
      ...item,
      id: item.id.toString(),
      description: getOperationDesc(item, currency, account!.rateUsd, account!.emissionMultiplier),
    }))
    setItems(items)
  }, [account, currency, operations, isLoading, isOperationsLoading, page, getOperationDesc])

  if (isLoading) {
    return <PageLoader />
  }

  if (!account) {
    return <Navigate to={"/"} />
  }

  return (
    <div className="flex flex-col gap-7 leading-[1.125rem]">
      <List items={{ id: account.address, ...account }} rows={rows} itemRenderer={listItemRenderer} />
      {items && !!items.length && (
        <OperationsTable
          title={t("operations")}
          totalItems={account.operations.totalItems}
          items={items}
          page={page}
          onPageChange={setPage}
          currency={currency}
          rateUsd={account.rateUsd}
          emissionMultiplier={account.emissionMultiplier}
        />
      )}
    </div>
  )
}
