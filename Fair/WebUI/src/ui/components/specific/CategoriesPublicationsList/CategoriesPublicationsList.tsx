import { CategoryPublications as CategoryPublicationsType } from "types"

import { CategoryPublicationsList } from "./CategoryPublicationsList"
import { CategoriesPublicationsListChildrenProps } from "./types"

type CategoriesPublicationsListBaseProps = {
  siteId: string
  isPending: boolean
  categoriesPublications?: CategoryPublicationsType[]
}

export type CategoriesPublicationsListProps = CategoriesPublicationsListBaseProps &
  CategoriesPublicationsListChildrenProps

export const CategoriesPublicationsList = ({
  siteId,
  isPending,
  categoriesPublications,
  ...rest
}: CategoriesPublicationsListProps) => {
  return isPending || !categoriesPublications ? (
    <>⌛ LOADING</>
  ) : (
    <>
      {categoriesPublications.map(x => (
        <CategoryPublicationsList key={x.id} siteId={siteId} {...x} {...rest} />
      ))}
    </>
  )
}
