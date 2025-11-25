import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber, isString } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetReviewProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState, TextModal } from "ui/components"
import { getReviewsItemRenderer } from "ui/renderers"
import { parseInteger } from "utils"

import { ReviewEdit } from "types"

export const ReviewsTab = () => {
  const { t } = useTranslation("tabReviews")
  const { siteId } = useParams()

  const [selectedReviewId, setSelectedReviewId] = useState<string | undefined>()
  const [selectedReviewText, setSelectedReviewText] = useState<string | undefined>()
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
      { accessor: "votes", label: t("common:votes"), type: "votes" },
      { accessor: "anbb", label: t("common:anbb"), type: "anbb", title: t("common:anbbFull") },
      { accessor: "action", label: t("common:action"), type: "review-action", className: "w-[20%]" },
    ],
    [t],
  )

  const handleTableRowClick = useCallback(
    (id: string) => {
      setSelectedReviewId(id)

      const operation = reviews?.items.find(x => x.id === id)?.options[0].operation as ReviewEdit
      setSelectedReviewText(operation?.text)
    },
    [reviews],
  )

  const handleCloseModal = useCallback(() => {
    setSelectedReviewText(undefined)
    setSelectedReviewId(undefined)
  }, [])

  const handleApproveClick = useCallback(
    (value?: unknown) => {
      alert("approve " + (isString(value) ? value : selectedReviewId))
      handleCloseModal()
    },
    [handleCloseModal, selectedReviewId],
  )

  const handleRejectClick = useCallback(
    (value?: unknown) => {
      alert("reject " + (isString(value) ? value : selectedReviewId))
      handleCloseModal()
    },
    [handleCloseModal, selectedReviewId],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  const itemRenderer = useMemo(
    () => getReviewsItemRenderer(t, handleApproveClick, handleRejectClick),
    [handleApproveClick, handleRejectClick, t],
  )

  return (
    <>
      <div className="flex flex-col gap-6">
        <Table
          columns={columns}
          items={reviews?.items}
          itemRenderer={itemRenderer}
          tableBodyClassName="text-2sm leading-5"
          emptyState={<TableEmptyState message={t("noReviews")} />}
          onRowClick={handleTableRowClick}
        />
        <div className="flex w-full justify-end">
          <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
        </div>
      </div>
      {selectedReviewId && selectedReviewText && (
        <TextModal
          title={t("reviewModalTitle")}
          text={selectedReviewText}
          onCancel={handleRejectClick}
          onConfirm={handleApproveClick}
          confirmLabel={t("common:approve")}
          cancelLabel={t("common:reject")}
        />
      )}
    </>
  )
}
