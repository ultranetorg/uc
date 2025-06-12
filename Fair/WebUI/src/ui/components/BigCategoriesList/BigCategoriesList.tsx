import { ReactElement } from "react"
import { Link } from "react-router-dom"

import { BigCategoryCard } from "./BigCategoryCard"

export type BigCategoriesListItem = {
  id: string
  title: string
  icon: ReactElement
}

export type BigCategoriesListProps = {
  isLoading?: boolean
  siteId: string
  items?: BigCategoriesListItem[]
}

export const BigCategoriesList = ({ siteId, items }: BigCategoriesListProps) => (
  <div className="grid grid-cols-3 gap-4 xl:grid-cols-4">
    {items?.map(({ id, ...rest }) => (
      <Link to={`/${siteId}/c/${id}`} key={id}>
        <BigCategoryCard {...rest} />
      </Link>
    ))}
  </div>
)
