import { memo } from "react"
import { Link } from "react-router-dom"

import { SvgFolderSoftwareXl } from "assets"
import { CategoryParentBaseWithChildren } from "types"
import { buildFileUrl, formatTitle, routes } from "utils"

export type ChildCategoriesListProps = {
  categories: CategoryParentBaseWithChildren[]
}

export const ChildCategoriesList = memo(({ categories }: ChildCategoriesListProps) => (
  <div className="flex flex-col gap-4">
    {categories.map(x => (
      <Link
        key={x.id}
        to={routes.category(x.id)}
        className="flex w-full min-w-0 flex-col items-center gap-2 rounded-lg bg-gray-100 py-5"
        title={x.title}
      >
        <div className="size-8 overflow-hidden rounded-md">
          {x.avatarId ? (
            <img src={buildFileUrl(x.avatarId)} className="size-full object-cover" />
          ) : (
            <SvgFolderSoftwareXl className="size-full stroke-gray-800" />
          )}
        </div>
        <span className="text-2sm leading-4.5">{formatTitle(x.title)}</span>
      </Link>
    ))}
  </div>
))
