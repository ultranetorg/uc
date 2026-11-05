import { useCallback } from "react"
import { useTranslation } from "react-i18next"

import { ProposalComment, Review, TotalItemsResult } from "types"
import { CommentsList, CommentsSectionHeader, Pagination } from "ui/components"

import { ProposalCommentItem } from "./ProposalCommentItem"

export type CommentsSectionProps = {
  inputDisabled?: boolean
  inputLoading?: boolean
  showCommentInput?: boolean
  isPending: boolean
  comments: TotalItemsResult<ProposalComment> | undefined
  page: number
  pageSize: number
  onPageChange: (page: number) => void
  onCommentSubmit: (comment: string) => void
}

export const CommentsSection = ({
  inputDisabled,
  inputLoading,
  showCommentInput,
  isPending,
  comments,
  page,
  pageSize,
  onPageChange,
  onCommentSubmit,
}: CommentsSectionProps) => {
  const { t } = useTranslation("proposalView")
  const pagesCount = Math.ceil((comments?.totalItems ?? 0) / pageSize)

  const renderComment = useCallback(
    (comment: Review | ProposalComment) => ("proposalId" in comment ? <ProposalCommentItem comment={comment} /> : null),
    [],
  )

  return (
    <div className="flex flex-col gap-4">
      <CommentsSectionHeader label={t("common:comments")} totalItems={comments?.totalItems}>
        <Pagination page={page} pagesCount={pagesCount} onPageChange={onPageChange} />
      </CommentsSectionHeader>
      <CommentsList
        inputDisabled={inputDisabled}
        inputLoading={inputLoading}
        comments={comments?.items}
        isPending={isPending}
        commentInputPlaceholder={t("addComment")}
        noCommentsLabel={t("noComments")}
        showMoreCommentsLabel={t("showMore")}
        showCommentInput={showCommentInput}
        renderComment={renderComment}
        onCommentSubmit={onCommentSubmit}
      />
      <Pagination className="justify-end" page={page} pagesCount={pagesCount} onPageChange={onPageChange} />
    </div>
  )
}
