import { useCallback, useEffect } from "react"
import { useParams } from "react-router-dom"

import { PAGE_SIZES } from "constants"
import { useGetModeratorPublications } from "entities"
import { Input, Pagination, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const DisputesTab = () => {
  const { page, setPage, pageSize, setPageSize, search, setSearch, resetPagination } = usePagePagination()

  const { siteId } = useParams()
  const { isPending, isError, data: publications } = useGetModeratorPublications(siteId, page, pageSize, search)

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0 ? Math.ceil(publications.totalItems / pageSize) : 0

  useEffect(() => {
    return () => {
      resetPagination()
    }
  }, [resetPagination])

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

  return <div className="flex flex-col"></div>
}
