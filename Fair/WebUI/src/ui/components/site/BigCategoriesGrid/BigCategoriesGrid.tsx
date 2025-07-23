import { Link } from "react-router-dom"

import { BigCategoryCard } from "./BigCategoryCard"
import { BigCategoriesGridProps } from "./types"

export const BigCategoriesGrid = ({ siteId, items }: BigCategoriesGridProps) => (
  <div className="grid grid-cols-3 gap-4 xl:grid-cols-4">
    {items?.map(({ id, ...rest }) => (
      <Link to={`/${siteId}/c/${id}`} key={id}>
        <BigCategoryCard {...rest} />
      </Link>
    ))}
  </div>
)
