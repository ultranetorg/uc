import { ReactNode } from "react"
import { PropsWithClassName } from "types"

type TableBaseColumn = {
  accessor: string
  label?: string
  /** Types ending with "action-button" / "action-buttons" are drawn as 1 / 2 button placeholders in `TableSkeleton`. */
  type?: string
  title?: string
}

export type TableColumn = PropsWithClassName & TableBaseColumn

export type TableItem = {
  id: string
  [key: string]: unknown
}

export type TableItemRenderer = (item: TableItem, column: TableColumn) => ReactNode

export type TableProps = {
  columns: TableColumn[]
  emptyState?: ReactNode
  items?: TableItem[]
  tableBodyClassName?: string
  itemRenderer?: TableItemRenderer
  onRowClick?: (id: string) => void
  isRowClickable?: (item: TableItem) => boolean
}
