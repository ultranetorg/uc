import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetAuthorReferendums } from "entities"
import { useUrlParamsState } from "hooks"
import { ProposalsView } from "ui/views"
import { parseInteger } from "utils"

export const ReferendumsTab = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("governance")
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

  const { data: referendums } = useGetAuthorReferendums(siteId, page, DEFAULT_PAGE_SIZE_20, state.query)
  const pagesCount =
    referendums?.totalItems && referendums.totalItems > 0 ? Math.ceil(referendums.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: state.query, page })
      setPage(page)
    },
    [setState, state.query],
  )

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/g/r/${id}`), [navigate, siteId])

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
        proposals={referendums}
        page={page}
        pagesCount={pagesCount}
        search={state.query}
        onPageChange={handlePageChange}
        onTableRowClick={handleTableRowClick}
        onSearchChange={handleSearchChange}
      />
    </div>
  )
}
