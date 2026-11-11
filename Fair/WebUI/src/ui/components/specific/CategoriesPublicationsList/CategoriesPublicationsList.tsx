import { memo } from "react"
import { CategoryPublications as CategoryPublicationsType } from "types"
import { PublicationsGridSkeleton } from "ui/components/specific"

import { CategoryPublicationsList } from "./CategoryPublicationsList"
import { CategoriesPublicationsListChildrenProps } from "./types"

type CategoriesPublicationsListBaseProps = {
  storeId: string
  isLoading: boolean
  categoriesPublications?: CategoryPublicationsType[]
}

export type CategoriesPublicationsListProps = CategoriesPublicationsListBaseProps &
  CategoriesPublicationsListChildrenProps

export const CategoriesPublicationsList = memo(
  ({ storeId, isLoading, categoriesPublications, ...rest }: CategoriesPublicationsListProps) => {
    return isLoading || !categoriesPublications ? (
      <PublicationsGridSkeleton />
    ) : (
      <>
        {categoriesPublications.map(x => (
          <CategoryPublicationsList key={x.id} storeId={storeId} {...x} {...rest} />
        ))}
      </>
    )
  },
)
