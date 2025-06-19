import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetCategory, useGetCategoryPublications } from "entities"
import { Pagination } from "ui/components"
import { CategoriesList, PublicationsList } from "ui/components/specific"

import { CategoryHeader } from "./CategoryHeader"

export const CategoryPage = () => {
  const { t } = useTranslation("category")

  const { siteId, categoryId } = useParams()
  const { data: category, isPending } = useGetCategory(categoryId)
  const { data: publications, isPending: isPendingPublications } = useGetCategoryPublications(category?.id, 0)

  if (isPending || !category || isPendingPublications || !publications) {
    return <div>LOADING</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <CategoryHeader siteId={siteId!} category={category} />

      {category.categories.length !== 0 || publications.items.length !== 0 ? (
        <>
          <CategoriesList siteId={siteId!} categories={category.categories} />
          <PublicationsList isPending={isPendingPublications} publications={publications.items} siteId={siteId!} />
        </>
      ) : (
        <span className="text-center">{t("empty")}</span>
      )}

      <Pagination onPageChange={page => console.log(page)} page={1} pagesCount={10} />
    </div>
  )
}
