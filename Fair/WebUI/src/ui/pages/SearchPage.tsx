import { useCallback, useEffect } from "react"
import { Navigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useSearchQueryContext } from "app"
import { DEFAULT_PAGE_SIZE } from "config"
import { useSearchPublications } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination } from "ui/components"
import { PublicationsList, SearchPageHeader } from "ui/components/specific"
import { parseInteger } from "utils"

export const SearchPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("searchPage")

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
    query: {
      defaultValue: "",
      validate: v => v !== "",
    },
  })

  const { query: searchQuery } = useSearchQueryContext()

  const { isPending, data: publications } = useSearchPublications(siteId, state.page, state.query || searchQuery)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0 ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE) : 0

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: searchQuery, page })
    },
    [searchQuery, setState],
  )

  useEffect(() => {
    if (searchQuery !== state.query) {
      setState({ query: searchQuery, page: 0 })
    }
  }, [searchQuery, setState, state])

  if (!searchQuery) {
    return <Navigate to={`/${siteId}`} />
  }

  return (
    <div className="flex flex-col gap-6">
      <SearchPageHeader searchResultsCount={5} searchResultsLabel={t("searchResults")} />
      <PublicationsList
        publications={publications?.items}
        isLoading={isPending || !publications?.items}
        siteId={siteId!}
      />
      <Pagination onPageChange={handlePageChange} page={state.page} pagesCount={pagesCount} />
    </div>
  )
}
