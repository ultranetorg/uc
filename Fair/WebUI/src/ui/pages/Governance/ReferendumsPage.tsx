import { useEffect, useMemo } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthorReferendums } from "entities"
import { GovernanceModerationHeader } from "ui/components/specific"
import { usePagePagination } from "ui/pages/hooks"
import { getReferendumsRowRenderer as getRowRenderer } from "ui/renderers"
import { ProposalsTemplate } from "ui/templates"

export const ReferendumsPage = () => {
  const { page, setPage, pageSize, search, setSearch } = usePagePagination()
  const { siteId } = useParams()
  const { t } = useTranslation("referendums")

  const rowRenderer = useMemo(() => getRowRenderer(siteId!), [siteId])

  const { isPending, data: referendums } = useGetAuthorReferendums(siteId, page, pageSize, search)
  const pagesCount =
    referendums?.totalItems && referendums.totalItems > 0 ? Math.ceil(referendums.totalItems / pageSize) : 0

  useEffect(() => {
    if (!isPending && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isPending, page, pagesCount, setPage])

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
        search={search}
        tableRowRenderer={rowRenderer}
        onSearchChange={setSearch}
      />
    </div>
  )
}
