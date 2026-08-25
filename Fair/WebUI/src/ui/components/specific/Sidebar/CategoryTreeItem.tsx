import { memo } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { CategoryTreeItem as CategoryTreeItemType, formatTitle, routes } from "utils"
import { SvgChevronDown } from "assets"

type CategoryTreeItemBaseProps = {
  storeId: string
}

export type CategoryTreeItemProps = CategoryTreeItemType & CategoryTreeItemBaseProps

const ROOT_INDENT_PX = 30
const INDENT_PX = 14

export const CategoryTreeItem = memo(
  ({ storeId, id, title, depth, active, expanded, hasChildren }: CategoryTreeItemProps) => {
    return (
      <Link
        className={twMerge(
          "flex h-6 min-w-0 cursor-pointer select-none items-center rounded-md text-2xs leading-4 hover:bg-gray-100",
          active && "bg-gray-100",
        )}
        style={{ paddingLeft: ROOT_INDENT_PX + (depth - 1) * INDENT_PX }}
        to={routes.category(storeId, id)}
      >
        <span className="min-w-0 flex-1 truncate" title={title}>
          {formatTitle(title)}
        </span>
        {hasChildren && (
          <SvgChevronDown
            className={twMerge("shrink-0 -rotate-90 stroke-gray-500", expanded && "rotate-0 transform stroke-gray-800")}
          />
        )}
      </Link>
    )
  },
)
