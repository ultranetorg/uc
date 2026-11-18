import { memo } from "react"

import { TableBody } from "./TableBody"
import { TableHeader } from "./TableHeader"
import { TableProps } from "./types"
import { getTableMinWidth } from "./utils"

export const Table = memo(
  ({ columns, emptyState, items, tableBodyClassName, itemRenderer, onRowClick, isRowClickable }: TableProps) => (
    <div className="overflow-x-auto rounded-lg border border-gray-300">
      <table className="w-full table-fixed border-collapse bg-gray-100" style={{ minWidth: getTableMinWidth(columns) }}>
        <TableHeader columns={columns} />
        <TableBody
          items={items}
          emptyState={emptyState}
          columns={columns}
          tableBodyClassName={tableBodyClassName}
          itemRenderer={itemRenderer}
          onRowClick={onRowClick}
          isRowClickable={isRowClickable}
        />
      </table>
    </div>
  ),
)
