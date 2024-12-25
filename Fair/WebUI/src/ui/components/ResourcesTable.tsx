import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"

import { Breakpoints, DEFAULT_PAGE_SIZE } from "constants"
import { useMediaQuery } from "hooks"
import { Table, TableColumn, TableItem, TableFooter } from "ui/components"
import { getTableItemRenderer } from "ui/renderers"

const getResourcesColumns = (t: TFunction<string, undefined, string>, hasExtendedColumns: boolean): TableColumn[] => [
  { accessor: "address.resource", label: t("resourcesTable.resource"), className: "w-5/12 text-left" },
  { accessor: "data.type", label: t("resourcesTable.type"), className: "w-2/12 text-left" },
  ...(hasExtendedColumns
    ? [{ accessor: "data.value", label: t("resourcesTable.data"), className: "w-3/12 text-left" }]
    : []),
  ...(hasExtendedColumns
    ? [{ accessor: "data.length", label: t("resourcesTable.dataLength"), className: "w-2/12 text-right" }]
    : []),
]

type ResourcesTableProps = {
  totalItems: number
  items: TableItem[]
  page: number
  onPageChange: (page: number) => void
}

export const ResourcesTable = (props: ResourcesTableProps) => {
  const { totalItems, items, page, onPageChange } = props

  const { t } = useTranslation("components")
  const isMediumView = useMediaQuery(Breakpoints.md)

  const columns: TableColumn[] = useMemo(() => getResourcesColumns(t, !isMediumView), [t, isMediumView])

  const tableItemRenderer = useMemo(() => getTableItemRenderer(), [])

  return (
    <Table
      title={t("resourcesTable.title")}
      columns={columns}
      items={items}
      itemRenderer={tableItemRenderer}
      footer={
        <TableFooter pageSize={DEFAULT_PAGE_SIZE} page={page} onPageChange={onPageChange} totalItems={totalItems} />
      }
    />
  )
}
