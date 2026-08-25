import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { useLocalStorage } from "usehooks-ts"
import { isNumber } from "lodash"

import { useStoreContext } from "app"
import { CATEGORY_PUBLICATIONS_DEFAULT_PAGE_SIZE } from "config"
import { useGetCategoryDetails, useGetCategoryPublications } from "entities"
import { useParams, useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { NoContent, Pagination } from "ui/components"
import { PublicationsGrid, PublicationsList, ViewType } from "ui/components/specific"
import { parseInteger } from "utils"

import { CategoryHeader } from "./CategoryHeader"

export const CategoryPage = () => {
  const { categoryId } = useParams()
  const { store } = useStoreContext()
  const storeId = useResolveStoreId()
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

  useStoreTitle(store?.title, category?.title ? `Category - ${category?.title}` : undefined)

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / CATEGORY_PUBLICATIONS_DEFAULT_PAGE_SIZE)
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
      <CategoryHeader category={category} storeId={storeId!} view={view} onViewChange={handleViewChange} />
      {publications.items.length !== 0 ? (
        view === "grid" ? (
          <PublicationsGrid
            isPending={isPendingPublications}
            publications={publications.items}
            storeId={storeId!}
            productType={category.type}
          />
        ) : (
          <PublicationsList storeId={storeId!} publications={publications.items} />
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
