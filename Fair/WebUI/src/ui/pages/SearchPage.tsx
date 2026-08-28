import { useCallback, useEffect, useMemo } from "react"
import { Navigate } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useSearchQueryContext, useStoreContext } from "app"
import { useSearchPaginatedPublications } from "entities"
import { ProductType } from "types"
import { useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { MessageBox, NextPagination } from "ui/components"
import { PublicationsList, SearchPageHeader } from "ui/components/specific"
import { routes } from "utils"

export const SearchPage = () => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("searchPage")

  const [state, setState] = useUrlParamsState({
    categoriesIds: {
      defaultValue: [] as string[],
    },
    query: {
      defaultValue: "",
      validate: v => v !== "",
    },
    type: {
      defaultValue: "",
      validate: v => v === "software" || v === "movie" || v === "music" || v === "book" || v === "game",
    },
  })

  const { store } = useStoreContext()
  const { query: searchQuery, setQuery: setSearchQuery } = useSearchQueryContext()

  const pageTitle = state.query || searchQuery
  useStoreTitle(store?.title, pageTitle ? `Search - ${pageTitle}` : undefined)

  const {
    isPending,
    data: publications,
    page,
    loadedPagesCount,
    hasNext,
    isFetchingNext,
    onPageChange,
  } = useSearchPaginatedPublications(
    storeId,
    state.query || searchQuery,
    state.categoriesIds,
    state.type as ProductType,
  )

  const handleTypeChange = useCallback((type: ProductType | null) => setState({ type: type ?? "" }), [setState])

  const handleCategoriesChange = useCallback(
    (categoryIds: string[] | undefined) => setState({ categoriesIds: categoryIds ?? [] }),
    [setState],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: searchQuery })
      onPageChange(page)
    },
    [onPageChange, searchQuery, setState],
  )

  const initialCategoryIds = useMemo(
    () => (state.categoriesIds.length > 0 ? state.categoriesIds : undefined),
    [state.categoriesIds],
  )

  useEffect(() => {
    if (searchQuery && searchQuery !== state.query) {
      setState({ query: searchQuery })
    } else if (!searchQuery && state.query) {
      setSearchQuery(state.query)
    }
  }, [searchQuery, setSearchQuery, setState, state])

  if (!searchQuery && !state.query) {
    return <Navigate to={routes.store(storeId!)} />
  }

  return (
    <div className="flex flex-col gap-6">
      <SearchPageHeader
        searchResultsLabel={t("searchResults")}
        allCategoriesLabel={t("allCategories")}
        selectedType={(state.type as ProductType) || null}
        selectedTypeChange={handleTypeChange}
        initialCategoryIds={initialCategoryIds}
        selectedCategoriesChange={handleCategoriesChange}
      />
      {loadedPagesCount > 0 && publications.length > 0 ? (
        <>
          <PublicationsList
            publications={publications}
            isLoading={isPending || !publications}
            showPublicationType={state.type === ""}
          />
          <NextPagination
            hasNext={hasNext && !isFetchingNext}
            page={page}
            loadedPages={loadedPagesCount}
            onPageChange={handlePageChange}
          />
        </>
      ) : (
        <MessageBox className="p-6" message={t("noResults")} />
      )}
    </div>
  )
}
