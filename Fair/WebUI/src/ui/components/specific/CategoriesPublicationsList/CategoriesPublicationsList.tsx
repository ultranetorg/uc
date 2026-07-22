import { CategoryPublications as CategoryPublicationsType } from "types"

import { CategoryPublicationsList } from "./CategoryPublicationsList"
import { CategoriesPublicationsListChildrenProps } from "./types"

type CategoriesPublicationsListBaseProps = {
  storeId: string
  isPending: boolean
  categoriesPublications?: CategoryPublicationsType[]
}

export type CategoriesPublicationsListProps = CategoriesPublicationsListBaseProps &
  CategoriesPublicationsListChildrenProps

export const CategoriesPublicationsList = ({
  storeId,
  isPending,
  categoriesPublications,
  ...rest
}: CategoriesPublicationsListProps) => {
  return isPending || !categoriesPublications ? (
    <div>Loading</div>
  ) : (
    <>
      {categoriesPublications.map(x => (
        <CategoryPublicationsList key={x.id} siteId={storeId} {...x} {...rest} />
      ))}
    </>
  )
}
