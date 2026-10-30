import { memo } from "react"
import { Link } from "react-router-dom"

import { CategoryParentBaseWithChildren } from "types"
import { routes } from "utils"

import { ChildCategory } from "./ChildCategory"

export type ChildCategoriesListProps = {
  categories: CategoryParentBaseWithChildren[]
}

export const ChildCategoriesList = memo(({ categories }: ChildCategoriesListProps) => (
  <div className="grid grid-cols-2 gap-4">
    {categories.map(x => (
      <Link key={x.id} to={routes.category(x.id)} title={x.title}>
        <ChildCategory id={x.id} avatarId={x.avatarId} title={x.title} />
      </Link>
    ))}
  </div>
))
