import { useTranslation } from "react-i18next"

import { ProposalComment, TotalItemsResult } from "types"
import { CommentsList, CommentsSectionHeader } from "ui/components"

export type CommentsSectionProps = {
  isFetching: boolean
  comments: TotalItemsResult<ProposalComment>
}

export const CommentsSection = ({ isFetching, comments }: CommentsSectionProps) => {
  const { t } = useTranslation("proposal")

  return (
    <div className="flex flex-col gap-4">
      <CommentsSectionHeader label={t("common:comments")} totalItems={comments?.totalItems} />
      <CommentsList
        comments={comments?.items}
        isFetching={isFetching}
        commentInputPlaceholder={t("addComment")}
        noCommentsLabel={t("noComments")}
        showMoreCommentsLabel={t("showMore")}
        showCommentInput={true}
      />
    </div>
  )
}
