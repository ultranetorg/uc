import { Link } from "react-router-dom"

import { routes } from "utils"

import { BigCategoryCard } from "./BigCategoryCard"
import { BigCategoriesGridProps } from "./types"

export const BigCategoriesGrid = ({ items }: BigCategoriesGridProps) => (
  <div className="grid grid-cols-3 gap-4 xl:grid-cols-4">
    {items?.map(({ id, ...rest }) => (
      <Link to={routes.category(id)} key={id}>
        <BigCategoryCard id={id} {...rest} />
      </Link>
    ))}
  </div>
)
