import { useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetModeratorDiscussions } from "entities"
import { useUrlParamsState } from "hooks"
import { ModerationHeader } from "ui/components/specific"
import { ProposalsView } from "ui/views"
import { parseInteger } from "utils"

export const ProposalsPage = () => {
  const { t } = useTranslation("proposalsPage")
  const navigate = useNavigate()
  const { siteId } = useParams()

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

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/m/p/${id}`), [navigate, siteId])

  const handleSearchChange = useCallback(
    (query: string) => {
      setState({ query, page: 0 })
      setPage(0)
    },
    [setState],
  )

  return (
    <>
      <ModerationHeader title="Proposals" />
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
    </>
  )
}
