import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { isNumber, isString } from "lodash"

import { useStoreContext, useStorePoliciesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetReviewCreationsProposals, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useApprovalIdentityForOperation, useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { ProposalVoting, ReviewProposal, SpecialChoice } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { ReviewModal } from "ui/components/specific"
import { calculateVotesRequiredToWinProposal, isUserVoted, parseInteger, showToast } from "utils"

import { getReviewsPageItemRenderer } from "./reviewsPageItemRenderer"

export const ReviewsNewTab = () => {
  const { t } = useTranslation("reviewsPage")

  const creationApproval = useApprovalIdentityForOperation("review-creation")
  const editApproval = useApprovalIdentityForOperation("review-edit")
  const { invalidateOperation } = useInvalidation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { mutate } = useTransactMutationWithStatus()

  const canApproveOrReject = !!(creationApproval || editApproval)

  useStoreTitle(store?.title, "Reviews")

  const [selectedReviewId, setSelectedReviewId] = useState<string | undefined>()
  const [selectedReviewText, setSelectedReviewText] = useState<string | undefined>()
  const [selectedReviewRating, setSelectedReviewRating] = useState<number | undefined>()
  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)
  const [loadingItem, setLoadingItem] = useState<{ id: string; action: "approve" | "reject" } | undefined>()

  const { data: reviews, refetch } = useGetReviewCreationsProposals(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo<TableColumn[]>(
    () => [
      { accessor: "publication", label: t("columns:publication"), type: "publication", className: "w-[17%]" },
      { accessor: "author", label: t("columns:reviewer"), type: "account", className: "w-[15%]" },
      {
        accessor: "text",
        label: t("columns:text"),
        type: "text",
        className: canApproveOrReject ? "w-[23%]" : "w-[43%]",
      },
      {
        accessor: "rating",
        type: "rating",
        label: t("columns:rating"),
        className: "w-[8%] text-right",
      },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.proposalTitle"),
        type: "progress",
        className: "w-[12%]",
      },
      ...(canApproveOrReject
        ? [
            {
              accessor: "action",
              label: t("columns:action"),
              type: "review-action",
              className: "w-[20%] text-center",
            },
          ]
        : []),
    ],
    [t, canApproveOrReject],
  )

  const handleTableRowClick = useCallback(
    (id: string) => {
      const review = reviews?.items.find(x => x.id === id)
      setSelectedReviewId(id)
      setSelectedReviewText(review?.reviewText)
      setSelectedReviewRating(review?.rating)
    },
    [reviews],
  )

  const isRowClickable = useCallback(
    (item: TableItem) => {
      const review = item as ReviewProposal
      const approval = review.operation === "review-edit" ? editApproval : creationApproval
      return !!approval && !isUserVoted(approval.id, review)
    },
    [creationApproval, editApproval],
  )

  const handleCloseModal = useCallback(() => {
    setSelectedReviewText(undefined)
    setSelectedReviewRating(undefined)
    setSelectedReviewId(undefined)
  }, [])

  const vote = useCallback(
    (id: string, action: "approve" | "reject") => {
      setLoadingItem({ id: id, action })

      const proposal = reviews?.items.find(x => x.id === id)
      const approval = proposal?.operation === "review-edit" ? editApproval : creationApproval
      if (!approval) {
        setLoadingItem(undefined)
        return
      }

      const operation = new ProposalVoting(id, approval.id, action === "approve" ? 0 : SpecialChoice.Neither)
      mutate(operation, {
        onSuccess: () => {
          if (action === "approve") {
            const review = reviews?.items.find(x => x.id === id)
            invalidateOperation(review?.operation, { storeId: storeId!, publicationId: review?.publication?.id })
          }

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
    [creationApproval, editApproval, invalidateOperation, mutate, refetch, reviews?.items, storeId, t],
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

  const votesRequired = useMemo(
    () => ({
      "review-creation": calculateVotesRequiredToWinProposal("review-creation", store, policies),
      "review-edit": calculateVotesRequiredToWinProposal("review-edit", store, policies),
    }),
    [policies, store],
  )

  const itemRenderer = useMemo(
    () =>
      getReviewsPageItemRenderer(
        t,
        handleApproveClick,
        handleRejectClick,
        votesRequired,
        loadingItem,
        creationApproval?.id,
        editApproval?.id,
      ),
    [creationApproval?.id, editApproval?.id, handleApproveClick, handleRejectClick, loadingItem, t, votesRequired],
  )

  return (
    <>
      <Table
        columns={columns}
        items={reviews?.items}
        itemRenderer={itemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("noNewReviews")} />}
        onRowClick={canApproveOrReject && !loadingItem ? handleTableRowClick : undefined}
        isRowClickable={isRowClickable}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
      {selectedReviewId && selectedReviewText && selectedReviewRating !== undefined && (
        <ReviewModal
          size="compact"
          title={t("reviewModalTitle")}
          text={selectedReviewText}
          rating={selectedReviewRating}
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
