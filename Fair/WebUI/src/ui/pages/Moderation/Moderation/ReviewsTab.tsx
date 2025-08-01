import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_2 } from "config"
import { useGetReviewProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { itemRenderer } from "ui/renderers/reviewProposals"
import { parseInteger } from "utils"

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

  const { _isPending, _isError, data: reviews } = useGetReviewProposals(siteId, page, DEFAULT_PAGE_SIZE_2)

  const pagesCount =
    reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / DEFAULT_PAGE_SIZE_2) : 0

  const columns = useMemo(
    () => [
      { accessor: "creator", label: t("reviewer"), type: "account", className: "w-[15%]" },
      { accessor: "publication", label: t("common:publication"), type: "publication", className: "w-[17%]" },
      { accessor: "text", label: t("text"), type: "text", className: "w-[23%]" },
      { accessor: "rating", label: t("common:rating"), type: "rating", className: "w-[5%]" },
      { accessor: "creationTime", label: t("common:date"), type: "date", className: "w-[8%]" },
      { accessor: "action", label: t("common:action"), type: "approve-reject-action", className: "w-[17%]" },
    ],
    [t],
  )

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
        emptyState={<TableEmptyState message={t("noProposals")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
