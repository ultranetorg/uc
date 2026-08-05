import { memo } from "react"
import { Link } from "react-router-dom"

import { CategoryTreeItem, formatTitle, routes } from "utils"

export type CategoryTreeProps = {
  storeId: string
  items: CategoryTreeItem[]
}

const INDENT_PX = 12

export const CategoryTree = memo(({ storeId, items }: CategoryTreeProps) => (
  <div className="flex flex-col gap-2 pl-2">
    {items.map(item =>
      item.active ? (
        <span
          key={item.id}
          className="truncate text-2xs font-semibold leading-4 text-gray-800"
          style={{ paddingLeft: item.depth * INDENT_PX }}
          title={item.title}
        >
          {formatTitle(item.title)}
        </span>
      ) : (
        <Link
          key={item.id}
          to={routes.category(storeId, item.id)}
          className="truncate text-2xs font-medium leading-4 text-gray-500 hover:text-gray-800 hover:underline"
          style={{ paddingLeft: item.depth * INDENT_PX }}
          title={item.title}
        >
          {formatTitle(item.title)}
        </Link>
      ),
    )}
  </div>
))
