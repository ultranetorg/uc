import { CategoryPublications as CategoryPublicationsType } from "types"

import { CategoryPublicationsList } from "./CategoryPublicationsList"
import { CategoriesPublicationsListChildrenProps } from "./types"

type CategoriesPublicationsListBaseProps = {
  storeId: string
  isLoading: boolean
  categoriesPublications?: CategoryPublicationsType[]
}

export type CategoriesPublicationsListProps = CategoriesPublicationsListBaseProps &
  CategoriesPublicationsListChildrenProps

export const CategoriesPublicationsList = ({
  storeId,
  isLoading,
  categoriesPublications,
  ...rest
}: CategoriesPublicationsListProps) => {
  return isLoading || !categoriesPublications ? (
    <div>Loading{import.meta.env.DEV ? " (CategoriesPublicationsList)" : ""}</div>
  ) : (
    <>
      {categoriesPublications.map(x => (
        <CategoryPublicationsList key={x.id} storeId={storeId} {...x} {...rest} />
      ))}
    </>
  )
}
