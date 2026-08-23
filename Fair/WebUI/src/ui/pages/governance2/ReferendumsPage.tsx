import { useCallback, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber, startCase } from "lodash"

import { useStoreContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetAuthorReferendums } from "entities"
import { useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { ModerationHeader } from "ui/components/specific"
import { ProposalsView } from "ui/views"
import { parseInteger, routes } from "utils"

export const ReferendumsPage = () => {
  const storeId = useResolveStoreId()
  const navigate = useNavigate()
  const { t } = useTranslation("referendumsPage")
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Referendums")

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

  const { data: referendums } = useGetAuthorReferendums(storeId, page, DEFAULT_PAGE_SIZE_20, state.query)
  const pagesCount =
    referendums?.totalItems && referendums.totalItems > 0 ? Math.ceil(referendums.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: state.query, page })
      setPage(page)
    },
    [setState, state.query],
  )

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.governance.referendum(storeId!, id)),
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
    <div className="flex flex-col gap-6">
      <ModerationHeader title={startCase(t("common:publisherReferendums"))} />
      <ProposalsView
        t={t}
        proposals={referendums}
        page={page}
        pagesCount={pagesCount}
        search={state.query}
        onPageChange={handlePageChange}
        onTableRowClick={handleTableRowClick}
        onSearchChange={handleSearchChange}
        onSearchCancel={handleSearchCancel}
      />
    </div>
  )
}
