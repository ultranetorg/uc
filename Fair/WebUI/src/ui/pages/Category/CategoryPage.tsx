import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE } from "constants"
import { useGetCategory, useGetCategoryPublications } from "entities"
import { CategoriesList, PublicationsList } from "ui/components"

import { CategoryHeader } from "./CategoryHeader"
import { usePagePagination } from "../hooks"

export const CategoryPage = () => {
  const { t } = useTranslation("category")

  const { page, setPage } = usePagePagination()
  const { siteId, categoryId } = useParams()
  const { data: category, isPending } = useGetCategory(categoryId)
  const { data: publications, isPending: isPendingPublications } = useGetCategoryPublications(category?.id, page)

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0 ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE) : 0

  if (isPending || !category || isPendingPublications || !publications) {
    return <div>LOADING</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <CategoryHeader siteId={siteId!} category={category} pagination={{ page, pagesCount, onClick: setPage }} />

      {category.categories.length !== 0 || publications.items.length !== 0 ? (
        <>
          <CategoriesList siteId={siteId!} categories={category.categories} isPending={isPending} />
          <PublicationsList isPending={isPendingPublications} publications={publications.items} siteId={siteId!} />
        </>
      ) : (
        <span className="text-center">{t("empty")}</span>
      )}
    </div>
  )
}
