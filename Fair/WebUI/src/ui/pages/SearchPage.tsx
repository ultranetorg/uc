import { useCallback, useEffect } from "react"
import { Navigate } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useSearchQueryContext, useSiteContext } from "app"
import { useSearchPaginatedPublications } from "entities"
import { useResolveSiteId, useSiteTitle, useUrlParamsState } from "hooks"
import { NextPagination } from "ui/components"
import { PublicationsList, SearchPageHeader } from "ui/components/specific"
import { routes } from "utils"

export const SearchPage = () => {
  const siteId = useResolveSiteId()
  const { t } = useTranslation("search")

  const [state, setState] = useUrlParamsState({
    query: {
      defaultValue: "",
      validate: v => v !== "",
    },
  })

  const { site } = useSiteContext()
  const { query: searchQuery, setQuery: setSearchQuery } = useSearchQueryContext()

  const pageTitle = state.query || searchQuery
  useSiteTitle(site?.title, pageTitle ? `Search - ${pageTitle}` : undefined)

  const {
    isPending,
    data: publications,
    page,
    loadedPagesCount,
    hasNext,
    isFetchingNext,
    onPageChange,
  } = useSearchPaginatedPublications(siteId, state.query || searchQuery)

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: searchQuery })
      onPageChange(page)
    },
    [onPageChange, searchQuery, setState],
  )

  useEffect(() => {
    if (searchQuery && searchQuery !== state.query) {
      setState({ query: searchQuery })
    } else if (!searchQuery && state.query) {
      setSearchQuery(state.query)
    }
  }, [searchQuery, setSearchQuery, setState, state])

  if (!searchQuery && !state.query) {
    return <Navigate to={routes.site(siteId!)} />
  }

  return (
    <div className="flex flex-col gap-6">
      <SearchPageHeader
        searchResultsLabel={t("searchResults")}
        allAuthorsLabel={t("allAuthors")}
        allCategoriesLabel={t("allCategories")}
      />
      <PublicationsList publications={publications} isLoading={isPending || !publications} siteId={siteId!} />
      <NextPagination
        hasNext={hasNext && !isFetchingNext}
        page={page}
        loadedPages={loadedPagesCount}
        onPageChange={handlePageChange}
      />
    </div>
  )
}
