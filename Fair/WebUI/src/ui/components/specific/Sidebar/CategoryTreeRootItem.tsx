import { memo } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { SvgChevronDown, SvgFolderSoftwareXl } from "assets"
import { buildFileUrl, CategoryTreeItem, routes } from "utils"

type CategoryTreeRootItemBaseProps = {
  storeId: string
}

export type CategoryTreeRootItemProps = CategoryTreeItem & CategoryTreeRootItemBaseProps

export const CategoryTreeRootItem = memo(({ id, title, avatarId, active, expanded }: CategoryTreeRootItemProps) => {
  return (
    <Link
      className={twMerge(
        "mt-1 flex min-w-0 cursor-pointer select-none items-center gap-1 rounded-md p-1 text-2xs leading-4 first:mt-0 hover:bg-gray-100",
        active && "bg-gray-100",
      )}
      to={routes.category(id)}
    >
      <div className="size-5 shrink-0 overflow-hidden rounded-md">
        {avatarId ? (
          <img src={buildFileUrl(avatarId)} className="size-full object-cover" />
        ) : (
          <SvgFolderSoftwareXl className="size-5 stroke-gray-800" />
        )}
      </div>
      <span className="min-w-0 flex-1 truncate" title={title}>
        {title}
      </span>
      <SvgChevronDown
        className={twMerge(
          "shrink-0 -rotate-90 stroke-gray-500",
          (expanded || active) && "rotate-0 transform stroke-gray-800",
        )}
      />
    </Link>
  )
})
