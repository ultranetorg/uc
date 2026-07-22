import { memo } from "react"
import { Link } from "react-router-dom"

import { CategoryPublications as CategoryPublicationsType } from "types"
import { PublicationsGrid } from "ui/components/specific"
import { routes } from "utils"

import { CategoriesPublicationsListChildrenProps } from "./types"

type CategoryPublicationsListBaseProps = {
  storeId: string
}

export type CategoryPublicationsListProps = CategoryPublicationsType &
  CategoryPublicationsListBaseProps &
  CategoriesPublicationsListChildrenProps

export const CategoryPublicationsList = memo(
  ({ id, title, type, publications, storeId, seeAllLabel }: CategoryPublicationsListProps) => (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between text-gray-800">
        <span className="text-xl font-bold leading-6">{title}</span>
        <Link className="text-sm font-medium leading-4.25 hover:font-semibold" to={routes.category(storeId, id)}>
          {seeAllLabel}
        </Link>
      </div>
      <PublicationsGrid storeId={storeId} isPending={false} publications={publications} productType={type} />
    </div>
  ),
)
