import { useCallback, useEffect } from "react"
import { useParams } from "react-router-dom"

import { useSearchQueryContext } from "app"
import { PAGE_SIZES } from "constants"
import { useSearchPublications } from "entities"
import { Pagination, PublicationsList, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const SearchPage = () => {
  const { page, setPage, pageSize, setPageSize } = usePagePagination()
  const { query, onSearchEvent } = useSearchQueryContext()

  const { siteId } = useParams()
  const { isPending, data: publications } = useSearchPublications(siteId, page, query, true)

  useEffect(() => {
    const unsubscribe = onSearchEvent(() => {
      console.log("search event")
    })

    return unsubscribe
  }, [onSearchEvent])

  // TODO: fix.
  const pagesCount = 10

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

  if (isPending && publications === undefined) {
    return <h2>LOADING</h2>
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex gap-3">
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
        <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
      </div>
      <PublicationsList publications={publications!.items} isPending={isPending} siteId={siteId!} />
    </div>
  )
}
