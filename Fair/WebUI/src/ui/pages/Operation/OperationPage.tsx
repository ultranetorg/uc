import { useMemo } from "react"
import { Navigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { useGetOperation } from "entities"
import { PageLoader, List, ListRow } from "ui/components"
import { getListItemRenderer } from "ui/renderers"

import { rowsToOperationTypeMap } from "./constants"

export const OperationPage = () => {
  const { operationId } = useParams()
  useDocumentTitle(operationId ? `Operation - ${operationId} | Ultranet Explorer` : "Operation | Ultranet Explorer")

  const { data: operation, isLoading } = useGetOperation(operationId) // TODO: check accountId is present.
  const { currency } = useSettings()
  const { t } = useTranslation("operation")

  const rows: ListRow[] = useMemo(
    () =>
      operation && rowsToOperationTypeMap.has(operation.$type) ? rowsToOperationTypeMap.get(operation.$type)! : [],
    [operation],
  )

  const listItemRenderer = useMemo(
    () => getListItemRenderer(currency, operation?.rateUsd, operation?.emissionMultiplier, t, "operation"),
    [currency, operation?.rateUsd, operation?.emissionMultiplier, t],
  )

  if (isLoading) {
    return <PageLoader />
  }

  if (!operation) {
    return <Navigate to={"/"} />
  }

  return (
    <div className="flex flex-col gap-7 leading-[1.125rem]">
      <List items={operation} rows={rows} itemRenderer={listItemRenderer} />
    </div>
  )
}
