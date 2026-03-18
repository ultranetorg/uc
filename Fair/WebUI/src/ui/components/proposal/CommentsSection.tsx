import { useTranslation } from "react-i18next"

import { ProposalComment, TotalItemsResult } from "types"
import { CommentsList, CommentsSectionHeader } from "ui/components"

export type CommentsSectionProps = {
  inputDisabled?: boolean
  inputLoading?: boolean
  showCommentInput?: boolean
  isFetching: boolean
  comments: TotalItemsResult<ProposalComment> | undefined
  onCommentSubmit: (comment: string) => void
}

export const CommentsSection = ({
  inputDisabled,
  inputLoading,
  showCommentInput,
  isFetching,
  comments,
  onCommentSubmit,
}: CommentsSectionProps) => {
  const { t } = useTranslation("proposalView")

  return (
    <div className="flex flex-col gap-4">
      <CommentsSectionHeader label={t("common:comments")} totalItems={comments?.totalItems} />
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
    </div>
  )
}
