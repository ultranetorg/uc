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
      <div className="flex items-center justify-between">
        <span className="text-xl leading-6 font-bold">{title}</span>
        <Link className="text-sm leading-4.25 font-medium hover:font-semibold" to={routes.category(id)}>
          {seeAllLabel}
        </Link>
      </div>
      <PublicationsGrid storeId={storeId} isLoading={false} publications={publications} productType={type} />
    </div>
  ),
)
