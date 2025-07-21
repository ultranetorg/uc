import { useEffect, useMemo } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthorReferendums } from "entities"
import { Input, Pagination, Table } from "ui/components"
import { GovernanceModerationHeader } from "ui/components/specific"
import { TableEmptyState } from "ui/components/referendums"
import { usePagePagination } from "ui/pages/hooks"
import {
  getReferendumsItemRenderer as getItemRenderer,
  getReferendumsRowRenderer as getRowRenderer,
} from "ui/renderers"

export const ReferendumsPage = () => {
  const { page, setPage, pageSize, search, setSearch } = usePagePagination()
  const { siteId } = useParams()
  const { t } = useTranslation("referendums")

  const itemRenderer = useMemo(() => getItemRenderer(t), [t])
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
      <div className="flex flex-col justify-between gap-4 xl:flex-row">
        <Input
          placeholder={t("searchReferendum")}
          value={search}
          onChange={setSearch}
          id="referendums-search-input"
          className="w-full max-w-120"
        />
        <Pagination onPageChange={() => console.log("onPageChange")} page={2} pagesCount={10} />
      </div>
      <Table
        columns={[
          { accessor: "text", label: "Text", className: "w-[26%]" },
          { accessor: "createdBy", label: "Created By", type: "account-by", className: "w-[18%]" },
          { accessor: "expiration", label: "Days Left", className: "w-[7%]" },
          { accessor: "action", label: "Action", type: "option", className: "w-[26%]" },
          { accessor: "voting", label: "Current voting", type: "voting", className: "w-[11%]" },
        ]}
        items={referendums?.items}
        itemRenderer={itemRenderer}
        rowRenderer={rowRenderer}
        emptyState={<TableEmptyState message={t("noReferendums")} />}
      />
      <div className="flex justify-end">
        <Pagination onPageChange={() => console.log("onPageChange")} page={2} pagesCount={10} />
      </div>
    </div>
  )
}
