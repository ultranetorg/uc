import { ReactNode } from "react"
import { PropsWithClassName } from "types"

type TableBaseColumn = {
  accessor: string
  label?: string
  type?: string
}

export type TableColumn = PropsWithClassName & TableBaseColumn

export type TableItem = {
  id: string
  [key: string]: unknown
}

export type TableItemRenderer = (item: TableItem, column: TableColumn) => ReactNode

export type TableRowRenderer = (children: JSX.Element, item: TableItem) => JSX.Element

export type TableProps = {
  columns: TableColumn[]
  emptyState?: ReactNode
  items?: TableItem[]
  itemRenderer?: TableItemRenderer
  rowRenderer?: TableRowRenderer
}
