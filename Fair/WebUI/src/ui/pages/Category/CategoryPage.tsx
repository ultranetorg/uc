import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { useLocalStorage } from "usehooks-ts"
import { isNumber } from "lodash"

import { useSiteContext } from "app"
import { DEFAULT_PAGE_SIZE_24 } from "config"
import { useGetCategoryDetails, useGetCategoryPublications } from "entities"
import { useParams, useResolveSiteId, useSiteTitle, useUrlParamsState } from "hooks"
import { NoContent, Pagination } from "ui/components"
import { CategoriesList, PublicationsGrid, PublicationsList, ViewType } from "ui/components/specific"
import { parseInteger } from "utils"

import { CategoryHeader } from "./CategoryHeader"

export const CategoryPage = () => {
  const { categoryId } = useParams()
  const { site } = useSiteContext()
  const siteId = useResolveSiteId()
  const { t } = useTranslation("category")

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const { data: category, isPending, error } = useGetCategoryDetails(categoryId)
  if (error) throw error

  const { data: publications, isPending: isPendingPublications } = useGetCategoryPublications(category?.id, state.page)

  useSiteTitle(site?.title, category?.title ? `Category - ${category?.title}` : undefined)

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_24)
      : 0

  const [localStorageView, setLocalStorageView] = useLocalStorage<ViewType>("categoryPage.view", "grid")
  const [view, setView] = useState<ViewType>(localStorageView)

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page })
    },
    [setState],
  )

  const handleViewChange = useCallback(
    (name: string) => {
      setView(name as ViewType)
      setLocalStorageView(name as ViewType)
    },
    [setLocalStorageView],
  )

  if (isPending || !category || isPendingPublications || !publications) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <CategoryHeader category={category} siteId={siteId!} view={view} onViewChange={handleViewChange} />
      {category.categories.length > 0 && <CategoriesList siteId={siteId!} categories={category.categories} />}
      {publications.items.length !== 0 ? (
        view === "grid" ? (
          <PublicationsGrid
            isPending={isPendingPublications}
            publications={publications.items}
            siteId={siteId!}
            productType={category.type}
          />
        ) : (
          <PublicationsList siteId={siteId!} publications={publications.items} />
        )
      ) : (
        <NoContent>{t("empty")}</NoContent>
      )}

      <div className="flex justify-end">
        <Pagination onPageChange={handlePageChange} page={state.page} pagesCount={pagesCount} />
      </div>
    </div>
  )
}
