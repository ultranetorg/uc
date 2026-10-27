import { useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { isNumber, startCase } from "lodash"

import { useStoreContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetProposals } from "entities"
import { useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { ModerationHeader } from "ui/components/specific"
import { ProposalsView } from "ui/views"
import { parseInteger, routes } from "utils"

export const ProposalsPage = () => {
  const { t } = useTranslation("proposalsPage")
  const navigate = useNavigate()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Proposals")

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

  const { data: proposals } = useGetProposals(storeId, page, DEFAULT_PAGE_SIZE_20, state.query)
  const pagesCount =
    proposals?.totalItems && proposals.totalItems > 0 ? Math.ceil(proposals.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: state.query, page })
      setPage(page)
    },
    [setState, state.query],
  )

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.moderation.proposal(storeId!, id)),
    [navigate, storeId],
  )

  const handleSearchChange = useCallback(
    (query: string) => {
      setState({ query, page: 0 })
      setPage(0)
    },
    [setState],
  )

  const handleSearchCancel = useCallback(() => {
    setState({ query: "", page: 0 })
    setPage(0)
  }, [setState])

  return (
    <>
      <ModerationHeader title={startCase(t("common:proposals"))} />
      <ProposalsView
        t={t}
        proposals={proposals}
        page={page}
        pagesCount={pagesCount}
        search={state.query}
        onSearchCancel={handleSearchCancel}
        onPageChange={handlePageChange}
        onSearchChange={handleSearchChange}
        onTableRowClick={handleTableRowClick}
      />
    </>
  )
}
