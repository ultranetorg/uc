import { useEffect, useState } from "react"
import { Navigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useSearchQueryContext } from "app"
import { useSearchPublications } from "entities"
import { Pagination } from "ui/components"
import { SearchPageHeader, SearchPublicationsList } from "ui/components/specific"

export const SearchPage = () => {
  const { query: searchQuery, onSearchEvent } = useSearchQueryContext()
  const [query, setQuery] = useState(searchQuery)

  const { siteId } = useParams()
  const { t } = useTranslation("searchPage")

  const { isPending, data: publications } = useSearchPublications(siteId, 0, query)

  useEffect(() => {
    const unsubscribe = onSearchEvent(() => {
      setQuery(searchQuery)
    })

    return unsubscribe
  }, [onSearchEvent, searchQuery])

  if (!query) {
    return <Navigate to={`/${siteId}`} />
  }

  return (
    <div className="flex flex-col gap-8">
      <SearchPageHeader searchResultsCount={5} searchResultsLabel={t("searchResults")} />
      <SearchPublicationsList
        publications={publications?.items}
        isLoading={isPending || !publications?.items}
        siteId={siteId!}
      />
      <Pagination onPageChange={page => console.log(page)} page={0} pagesCount={10} />
    </div>
  )
}
