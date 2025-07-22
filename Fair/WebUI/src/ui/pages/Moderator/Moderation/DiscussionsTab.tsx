import { useEffect } from "react"
import { useParams } from "react-router-dom"

import { useGetModeratorPublications } from "entities"
import { usePagePagination } from "ui/pages/hooks"

export const DiscussionsTab = () => {
  const { page, setPage, pageSize, search, resetPagination } = usePagePagination()

  const { siteId } = useParams()
  const { isPending, data: publications } = useGetModeratorPublications(siteId, page, pageSize, search)

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

  return <div className="flex flex-col"></div>
}
