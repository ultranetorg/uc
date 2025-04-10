import { useCallback, useEffect, useState } from "react"
import { Link, useParams, useSearchParams } from "react-router-dom"

import { useSearchContext } from "app"
import { DEFAULT_PAGE_SIZE, PAGE_SIZES } from "constants"
import { useSearchPublications } from "entities"
import { useQueryParams } from "hooks"
import { Pagination, PublicationCard, Select, SelectItem } from "ui/components"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const SearchPage = () => {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)

  const { search: contextSearch, setSearch: setContextSearch } = useSearchContext()
  const { siteId } = useParams()
  const { isPending, data: publications } = useSearchPublications(siteId, page, pageSize, contextSearch, true)

  const { page: queryPage, pageSize: querySize, search: querySearch } = useQueryParams()
  const [searchParams, setSearchParams] = useSearchParams()

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0 ? Math.ceil(publications.totalItems / pageSize) : 0

  useEffect(() => {
    if (page != queryPage) {
      setPage(queryPage)
    }
    if (pageSize != querySize) {
      setPageSize(querySize)
    }
    if (contextSearch != querySearch) {
      setContextSearch(querySearch)
    }
  }, [])

  useEffect(() => {
    if (!isPending && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isPending, page, pagesCount])

  useEffect(() => {
    if (page !== 0) {
      searchParams.set("page", page.toString())
    } else {
      searchParams.delete("page")
    }
    if (pageSize !== DEFAULT_PAGE_SIZE) {
      searchParams.set("pageSize", pageSize.toString())
    } else {
      searchParams.delete("pageSize")
    }
    if (contextSearch !== "") {
      searchParams.set("search", contextSearch)
    } else {
      searchParams.delete("search")
    }
    setSearchParams(searchParams)
  }, [page, pageSize, searchParams, contextSearch, setSearchParams])

  const handlePageSizeChange = useCallback((value: string) => {
    setPage(0)
    setPageSize(parseInt(value))
  }, [])

  return (
    <div className="flex flex-col gap-3">
      <div className="flex gap-3">
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
        <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
      </div>
      {isPending || !publications ? (
        <h2>Loading</h2>
      ) : publications.items.length > 0 ? (
        <div className="flex w-full flex-wrap gap-x-6 gap-y-6">
          {publications.items.map(p => (
            <Link to={`/${siteId}/p/${p.id}`} key={p.id}>
              <PublicationCard publicationName={p.productTitle} authorTitle={p.authorTitle} />
            </Link>
          ))}
        </div>
      ) : (
        <h2>Not found</h2>
      )}
    </div>
  )
}
