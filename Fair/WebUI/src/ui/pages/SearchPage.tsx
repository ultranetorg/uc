import { useCallback, useEffect } from "react"
import { Link, useParams } from "react-router-dom"

import { useSearchContext } from "app"
import { PAGE_SIZES } from "constants"
import { useSearchPublications } from "entities"
import { Pagination, PublicationCard, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const SearchPage = () => {
  const { page, setPage, pageSize, setPageSize } = usePagePagination()
  const { search: contextSearch } = useSearchContext()

  const { siteId } = useParams()
  const { isPending, data: publications } = useSearchPublications(siteId, page, pageSize, contextSearch, true)

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0 ? Math.ceil(publications.totalItems / pageSize) : 0

  useEffect(() => {
    if (!isPending && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isPending, page, pagesCount, setPage])

  const handlePageSizeChange = useCallback(
    (value: string) => {
      setPage(0)
      setPageSize(parseInt(value))
    },
    [setPage, setPageSize],
  )

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
