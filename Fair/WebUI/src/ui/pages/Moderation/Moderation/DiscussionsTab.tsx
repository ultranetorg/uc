import { useEffect, useMemo } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDiscussions } from "entities"
import { usePagePagination } from "ui/pages/hooks"
import { getDiscussionsRowRowRenderer as getRowRenderer } from "ui/renderers"
import { ProposalsView } from "ui/views"

export const DiscussionsTab = () => {
  const { page, setPage, pageSize, search, resetPagination, setSearch } = usePagePagination()

  const { siteId } = useParams()
  const { t } = useTranslation("discussions")
  const { isPending, data: discussions } = useGetModeratorDiscussions(siteId, page, pageSize, search)

  const rowRenderer = useMemo(() => getRowRenderer(siteId!), [siteId])

  const pagesCount =
    discussions?.totalItems && discussions.totalItems > 0 ? Math.ceil(discussions.totalItems / pageSize) : 0

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

  return (
    <div className="flex flex-col gap-6">
      <ProposalsView
        t={t}
        proposals={discussions}
        search={search}
        tableRowRenderer={rowRenderer}
        onSearchChange={setSearch}
      />
    </div>
  )
}
