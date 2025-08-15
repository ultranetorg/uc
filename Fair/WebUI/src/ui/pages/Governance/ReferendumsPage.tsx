import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useGetAuthorReferendums } from "entities"
import { useUrlParamsState } from "hooks"
import { GovernanceModerationHeader } from "ui/components/specific"
import { getReferendumsRowRenderer as getRowRenderer } from "ui/renderers"
import { ProposalsTemplate } from "ui/templates"
import { parseInteger } from "utils"
import { DEFAULT_PAGE_SIZE_20 } from "config"

export const ReferendumsPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("referendums")

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

  const rowRenderer = useMemo(() => getRowRenderer(siteId!), [siteId])

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: state.query, page })
      setPage(page)
    },
    [setState, state.query],
  )

  const handleSearchChange = useCallback(
    (query: string) => {
      setState({ query, page: 0 })
      setPage(0)
    },
    [setState],
  )

  return (
    <div className="flex flex-col gap-6">
      <GovernanceModerationHeader
        siteId={siteId!}
        title={t("title")}
        onCreateButtonClick={() => console.log("GovernanceModerationHeader")}
        homeLabel={t("common:home")}
        createButtonLabel={t("createReferendum")}
      />
      <ProposalsTemplate
        t={t}
        proposals={referendums}
        page={page}
        pagesCount={pagesCount}
        search={state.query}
        tableRowRenderer={rowRenderer}
        onPageChange={handlePageChange}
        onSearchChange={handleSearchChange}
      />
    </div>
  )
}
