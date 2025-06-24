import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetCategory, useGetCategoryPublications } from "entities"
import { Pagination } from "ui/components"
import { CategoriesList, PublicationsGrid, PublicationsList, ViewType } from "ui/components/specific"

import { CategoryHeader } from "./CategoryHeader"
import { useCallback, useState } from "react"
import { useLocalStorage } from "usehooks-ts"

export const CategoryPage = () => {
  const { t } = useTranslation("category")

  const { siteId, categoryId } = useParams()
  const { data: category, isPending } = useGetCategory(categoryId)
  const { data: publications, isPending: isPendingPublications } = useGetCategoryPublications(category?.id, 0)

  const [localStorageView, setLocalStorageView] = useLocalStorage<ViewType>("categoryPage.view", "grid")
  const [view, setView] = useState<ViewType>(localStorageView)

  const handleViewChange = useCallback(
    (name: string) => {
      setView(name as ViewType)
      setLocalStorageView(name as ViewType)
    },
    [setLocalStorageView],
  )

  if (isPending || !category || isPendingPublications || !publications) {
    return <div>LOADING</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <CategoryHeader category={category} siteId={siteId!} view={view} onViewChange={handleViewChange} />
      {category.categories.length > 0 && <CategoriesList siteId={siteId!} categories={category.categories} />}
      {publications.items.length !== 0 ? (
        view === "grid" ? (
          <PublicationsGrid isPending={isPendingPublications} publications={publications.items} siteId={siteId!} />
        ) : (
          <PublicationsList siteId={siteId!} publications={publications.items} />
        )
      ) : (
        <span className="text-center">{t("empty")}</span>
      )}

      <div className="flex justify-end">
        <Pagination onPageChange={page => console.log(page)} page={1} pagesCount={10} />
      </div>
    </div>
  )
}
