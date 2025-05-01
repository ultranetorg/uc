import { useParams } from "react-router-dom"

import { useGetCategory, useGetCategoryPublications } from "entities"
import { CategoriesList, PublicationsList } from "ui/components"

import { CategoryHeader } from "./CategoryHeader"

export const CategoryPage = () => {
  const { siteId, categoryId } = useParams()
  const { data: category, isPending } = useGetCategory(categoryId)
  const { data: publications, isPending: isPendingPublications } = useGetCategoryPublications(category?.id)

  if (isPending || !category || isPendingPublications || !publications) {
    return <div>LOADING</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <CategoryHeader siteId={siteId!} category={category} />

      <CategoriesList siteId={siteId!} categories={category.categories} isPending={isPending} />

      <span className="text-black">PUBLICATIONS:</span>
      <PublicationsList isPending={isPendingPublications} publications={publications.items} siteId={siteId!} />
    </div>
  )
}
