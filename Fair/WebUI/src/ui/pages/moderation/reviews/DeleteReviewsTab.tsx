import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { isNumber, isString } from "lodash"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetReviewDeletionsProposals, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useApprovalIdentityForOperation, useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { ProposalVoting, SpecialChoice } from "types"
import { Pagination, Table, TableEmptyState, TextModal } from "ui/components"
import { calculateVotesRequiredToWinProposal, parseInteger, showToast } from "utils"

import { getReviewsPageItemRenderer } from "./reviewsPageItemRenderer"

export const DeleteReviewsTab = () => {
  const approval = useApprovalIdentityForOperation("review-deletion")
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { t } = useTranslation("reviewsPage")

  useStoreTitle(store?.title, "Reviews")

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

  const { data: reviews, refetch } = useGetReviewDeletionsProposals(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const { mutate } = useTransactMutationWithStatus()
  const { invalidateOperation } = useInvalidation()

  const columns = useMemo(
    () => [
      { accessor: "author", label: t("common:reviewer"), type: "account", className: "w-[15%]" },
      { accessor: "publication", label: t("common:publication"), type: "publication", className: "w-[17%]" },
      { accessor: "text", label: t("common:text"), type: "text", className: "w-[23%]" },
      { accessor: "votes", label: t("common:votes"), type: "votes", className: "first-letter:uppercase text-center" },
      ...(approval && (isModerator || isPublisher)
        ? [{ accessor: "action", label: t("common:action"), type: "delete-actions", className: "w-[20%] text-center" }]
        : []),
    ],
    [isModerator, isPublisher, t, approval],
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

      const operation = new ProposalVoting(id, approval!.id, action === "approve" ? 0 : SpecialChoice.Neither)
      mutate(operation, {
        onSuccess: () => {
          if (action === "approve") {
            const review = reviews?.items.find(x => x.id === id)
            invalidateOperation("review-deletion", { storeId: storeId!, publicationId: review?.publication?.id })
          }

          const message = action === "approve" ? t("toast:reviewDeletionApproved") : t("toast:reviewDeletionRejected")
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
    [invalidateOperation, mutate, refetch, reviews, storeId, t, approval],
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
    () => ({ "review-deletion": calculateVotesRequiredToWinProposal("review-deletion", store, policies) }),
    [policies, store],
  )

  const itemRenderer = useMemo(
    () =>
      getReviewsPageItemRenderer(t, handleApproveClick, handleRejectClick, votesRequired, loadingItem, approval?.id),
    [handleApproveClick, handleRejectClick, loadingItem, t, approval, votesRequired],
  )

  return (
    <>
      <Table
        columns={columns}
        items={reviews?.items}
        itemRenderer={itemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("newReviewDeletions")} />}
        onRowClick={approval && !loadingItem ? handleTableRowClick : undefined}
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
