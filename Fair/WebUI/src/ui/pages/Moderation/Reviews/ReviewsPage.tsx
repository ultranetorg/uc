import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { isNumber, isString } from "lodash"

import { useModerationContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetReviewProposals } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { useUrlParamsState } from "hooks"
import { ProposalVoting } from "types"
import { Pagination, Table, TableEmptyState, TextModal } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { getReviewsItemRenderer } from "ui/renderers"
import { parseInteger, showToast } from "utils"

export const ReviewsPage = () => {
  const { siteId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { t } = useTranslation("reviewsPage")
  const voterId = getOperationVoterId("review-creation")

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
  const [loadingItem, setLoadingItem] = useState<{ id: string; action: "approve" | "reject" } | undefined>()

  const { data: reviews, refetch } = useGetReviewProposals(siteId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const { mutate } = useTransactMutationWithStatus()

  const columns = useMemo(
    () => [
      { accessor: "author", label: t("common:reviewer"), type: "account", className: "w-[15%]" },
      { accessor: "publication", label: t("common:publication"), type: "publication", className: "w-[17%]" },
      { accessor: "text", label: t("common:text"), type: "text", className: "w-[23%]" },
      { accessor: "votes", label: t("common:votes"), type: "votes" },
      { accessor: "nabb", label: t("common:nabb"), type: "nabb", title: t("common:nabbFull") },
      ...(voterId
        ? [{ accessor: "action", label: t("common:action"), type: "review-action", className: "w-[20%]" }]
        : []),
    ],
    [t, voterId],
  )

  const handleTableRowClick = useCallback(
    (id: string) => {
      setSelectedReviewId(id)
      const review = reviews?.items.find(x => x.id === id)
      setSelectedReviewText(review?.reviewText)
    },
    [reviews],
  )

  const handleCloseModal = useCallback(() => {
    setSelectedReviewText(undefined)
    setSelectedReviewId(undefined)
  }, [])

  const vote = useCallback(
    (id: string, action: "approve" | "reject") => {
      setLoadingItem({ id: id, action })

      const operation = new ProposalVoting(id, voterId!, action === "approve" ? 0 : -1)
      mutate(operation, {
        onSuccess: () => {
          const message = action === "approve" ? t("toast:reviewApproved") : t("toast:reviewRejected")
          showToast(message, "success")
        },
        onError: err => {
          showToast(err.toString(), "error")
        },
        onSettled: () => {
          setLoadingItem(undefined)
          refetch()
        },
      })
    },
    [mutate, refetch, t, voterId],
  )

  const handleApproveClick = useCallback(
    (value?: unknown) => {
      const reviewId = isString(value) ? value : selectedReviewId
      vote(reviewId!, "approve")
      handleCloseModal()
    },
    [handleCloseModal, selectedReviewId, vote],
  )

  const handleRejectClick = useCallback(
    (value?: unknown) => {
      const reviewId = isString(value) ? value : selectedReviewId
      vote(reviewId!, "reject")
      handleCloseModal()
    },
    [handleCloseModal, selectedReviewId, vote],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  const itemRenderer = useMemo(
    () => getReviewsItemRenderer(t, handleApproveClick, handleRejectClick, loadingItem),
    [handleApproveClick, handleRejectClick, loadingItem, t],
  )

  return (
    <>
      <ModerationHeader title={t("title")} parentBreadcrumbs={{ path: `/${siteId}/m`, title: t("common:proposals") }} />
      <Table
        columns={columns}
        items={reviews?.items}
        itemRenderer={itemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("noReviews")} />}
        onRowClick={voterId && !loadingItem ? handleTableRowClick : undefined}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
      {selectedReviewId && selectedReviewText && (
        <TextModal
          size="compact"
          title={t("reviewModalTitle")}
          text={selectedReviewText}
          onCancel={handleRejectClick}
          onConfirm={handleApproveClick}
          onClose={handleCloseModal}
          confirmLabel={t("common:approve")}
          cancelLabel={t("common:reject")}
        />
      )}
    </>
  )
}
