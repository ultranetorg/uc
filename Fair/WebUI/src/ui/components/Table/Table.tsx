import { TableBody } from "./TableBody"
import { TableHeader } from "./TableHeader"
import { TableProps } from "./types"

export const Table = ({ columns, emptyState, items, tableBodyClassName, itemRenderer, onRowClick }: TableProps) => (
  <div className="overflow-hidden rounded-lg border border-gray-300">
    <table className="w-full table-fixed border-collapse bg-gray-100">
      <TableHeader columns={columns} />
      <TableBody
        items={items}
        emptyState={emptyState}
        columns={columns}
        tableBodyClassName={tableBodyClassName}
        itemRenderer={itemRenderer}
        onRowClick={onRowClick}
      />
    </table>
  </div>
)
