import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetReviewProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { getReviewsItemRenderer } from "ui/renderers"
import { parseInteger } from "utils"

import { getCommonColumns } from "./constants"

export const ReviewsTab = () => {
  const { t } = useTranslation("tabReviews")
  const { siteId } = useParams()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: reviews } = useGetReviewProposals(siteId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      { accessor: "author", label: t("common:reviewer"), type: "account", className: "w-[15%]" },
      { accessor: "publication", label: t("common:publication"), type: "publication", className: "w-[17%]" },
      { accessor: "text", label: t("common:text"), type: "text", className: "w-[23%]" },
      { accessor: "action", label: t("common:action"), type: "action-short", className: "w-[13%]" },
      ...getCommonColumns(t),
    ],
    [t],
  )
  const itemRenderer = useMemo(() => getReviewsItemRenderer(t), [t])

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  return (
    <div className="flex flex-col gap-6">
      <Table
        columns={columns}
        items={reviews?.items}
        itemRenderer={itemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("noReviews")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
