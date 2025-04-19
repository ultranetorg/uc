import { useParams } from "react-router-dom"

import { useGetCategory, useGetCategoryPublications } from "entities"
import { CategoriesList, PublicationsList } from "ui/components"

export const CategoryPage = () => {
  const { siteId, categoryId } = useParams()
  const { data: category, isPending } = useGetCategory(categoryId)
  const { data: publications, isPending: isPendingPublications } = useGetCategoryPublications(category?.id)

  if (isPending || !category || isPendingPublications || !publications) {
    return <div>LOADING</div>
  }

  return (
    <div className="flex flex-col">
      <span className="text-black">CATEGORIES:</span>
      <CategoriesList siteId={siteId!} categories={category.categories} isPending={isPending} />

      <span className="text-black">PUBLICATIONS:</span>
      <PublicationsList isPending={isPendingPublications} publications={publications.items} />
    </div>
  )
}
