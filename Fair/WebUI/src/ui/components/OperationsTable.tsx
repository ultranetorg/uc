import { memo, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"

import { Breakpoints, DEFAULT_PAGE_SIZE } from "constants"
import { useMediaQuery } from "hooks"
import { Currency } from "types"
import { Table, TableColumn, TableItem, TableFooter } from "ui/components"
import { getTableItemRenderer } from "ui/renderers"

const getOperationColumns = (
  t: TFunction<string, undefined, string>,
  isXSmallView: boolean,
  isSmallView: boolean,
): TableColumn[] => {
  if (isXSmallView) {
    return [{ accessor: "id", label: t("operationsTable.id"), className: "w-full text-right" }]
  }

  if (isSmallView) {
    return [
      { accessor: "id", label: t("operationsTable.id"), className: "w-4/12 text-right" },
      { accessor: "$type", label: t("operationsTable.type"), className: "w-8/12 text-left" },
    ]
  }

  return [
    { accessor: "id", label: t("operationsTable.id"), className: "w-2/12 text-right" },
    { accessor: "$type", label: t("operationsTable.type"), className: "w-3/12 text-left" },
    { accessor: "description", label: t("operationsTable.description"), className: "w-7/12 text-left" },
  ]
}

type OperationsTableProps = {
  title: string | null
  totalItems: number
  items: TableItem[]
  page: number
  onPageChange: (page: number) => void
  currency?: Currency
  rateUsd?: number
  emissionMultiplier?: number
}

export const OperationsTable = memo((props: OperationsTableProps) => {
  const { title, totalItems, items, page, onPageChange, currency, rateUsd, emissionMultiplier } = props

  const { t } = useTranslation("components")
  const isXSmallView = useMediaQuery(Breakpoints.xs)
  const isSmallView = useMediaQuery(Breakpoints.sm)

  const columns: TableColumn[] = useMemo(
    () => getOperationColumns(t, isXSmallView, isSmallView),
    [t, isXSmallView, isSmallView],
  )

  const tableItemRenderer = useMemo(
    () => getTableItemRenderer("operations", undefined, currency, rateUsd, emissionMultiplier),
    [currency, rateUsd, emissionMultiplier],
  )

  return (
    <Table
      title={title}
      columns={columns}
      items={items || []}
      itemRenderer={tableItemRenderer}
      footer={
        <TableFooter pageSize={DEFAULT_PAGE_SIZE} page={page} onPageChange={onPageChange} totalItems={totalItems} />
      }
    />
  )
})
