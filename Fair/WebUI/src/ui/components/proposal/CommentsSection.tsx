import { useTranslation } from "react-i18next"

import { ProposalComment, TotalItemsResult } from "types"
import { CommentsList, CommentsSectionHeader, Pagination } from "ui/components"

export type CommentsSectionProps = {
  inputDisabled?: boolean
  inputLoading?: boolean
  showCommentInput?: boolean
  isFetching: boolean
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
  isFetching,
  comments,
  page,
  pageSize,
  onPageChange,
  onCommentSubmit,
}: CommentsSectionProps) => {
  const { t } = useTranslation("proposalView")
  const pagesCount = Math.ceil((comments?.totalItems ?? 0) / pageSize)

  return (
    <div className="flex flex-col gap-4">
      <CommentsSectionHeader label={t("common:comments")} totalItems={comments?.totalItems}>
        {pagesCount > 1 && <Pagination page={page} pagesCount={pagesCount} onPageChange={onPageChange} />}
      </CommentsSectionHeader>
      <CommentsList
        inputDisabled={inputDisabled}
        inputLoading={inputLoading}
        comments={comments?.items}
        isFetching={isFetching}
        commentInputPlaceholder={t("addComment")}
        noCommentsLabel={t("noComments")}
        showMoreCommentsLabel={t("showMore")}
        showCommentInput={showCommentInput}
        onCommentSubmit={onCommentSubmit}
      />
      {pagesCount > 1 && (
        <Pagination className="justify-end" page={page} pagesCount={pagesCount} onPageChange={onPageChange} />
      )}
    </div>
  )
}
