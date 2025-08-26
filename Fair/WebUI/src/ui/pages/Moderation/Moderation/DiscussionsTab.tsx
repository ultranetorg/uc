import { useCallback, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetModeratorDiscussions } from "entities"
import { useUrlParamsState } from "hooks"
import { ProposalsView } from "ui/views"
import { parseInteger } from "utils"

export const DiscussionsTab = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("tabDiscussions")
  const navigate = useNavigate()

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
  const [page, setPage] = useState(state.page)

  const { data: discussions } = useGetModeratorDiscussions(siteId, page, DEFAULT_PAGE_SIZE_20, state.query)
  const pagesCount =
    discussions?.totalItems && discussions.totalItems > 0 ? Math.ceil(discussions.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: state.query, page })
      setPage(page)
    },
    [setState, state.query],
  )

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/m-d/${id}`), [navigate, siteId])

  const handleSearchChange = useCallback(
    (query: string) => {
      setState({ query, page: 0 })
      setPage(0)
    },
    [setState],
  )

  return (
    <div className="flex flex-col gap-6">
      <ProposalsView
        t={t}
        proposals={discussions}
        page={page}
        pagesCount={pagesCount}
        search={state.query}
        onPageChange={handlePageChange}
        onSearchChange={handleSearchChange}
        onTableRowClick={handleTableRowClick}
      />
    </div>
  )
}
