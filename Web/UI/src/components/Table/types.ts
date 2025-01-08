import { ReactNode } from "react"

import { Breakpoints } from "constants/"
import { PropsWithClassName } from "types"

export type TableItemRenderer = (
  value: any,
  item: TableItem,
  column?: TableColumn,
  breakpoint?: Breakpoints,
) => ReactNode

type TableColumnBase = { accessor: string; label?: string | null; type?: string | undefined }

export type TableColumn = PropsWithClassName<TableColumnBase>

export type TableItem = {
  id: string
  [key: string]: any
}
