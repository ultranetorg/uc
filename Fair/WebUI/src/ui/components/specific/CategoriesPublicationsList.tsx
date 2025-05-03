import { CategoryPublications as CategoryPublicationsType } from "types"

import { CategoryPublicationsList } from "./CategoryPublicationsList"

export type CategoriesPublicationsListProps = {
  siteId: string
  isPending: boolean
  categoriesPublications?: CategoryPublicationsType[]
}

export const CategoriesPublicationsList = ({
  siteId,
  isPending,
  categoriesPublications,
}: CategoriesPublicationsListProps) => {
  return isPending || !categoriesPublications ? (
    <>⌛ LOADING</>
  ) : (
    <>
      {categoriesPublications.map(x => (
        <CategoryPublicationsList key={x.id} siteId={siteId} {...x} />
      ))}
    </>
  )
}
