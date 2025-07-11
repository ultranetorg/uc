import { TableBody } from "./TableBody"
import { TableHeader } from "./TableHeader"
import { TableProps } from "./types"

export const Table = ({ columns, emptyState, items, itemRenderer }: TableProps) => (
  <div className="w-full table-fixed border-collapse flex-col divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
    <TableHeader columns={columns} />
    <TableBody items={items} emptyState={emptyState} columns={columns} itemRenderer={itemRenderer} />
  </div>
)
