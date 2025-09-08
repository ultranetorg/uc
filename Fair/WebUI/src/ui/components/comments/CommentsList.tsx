import { memo } from "react"

import { ProposalComment, Review } from "types"
import { ButtonOutline } from "ui/components"

import { Comment } from "./Comment"
import { CommentInput } from "./CommentInput"
import { CommentsListEmptyState } from "./CommentsListEmptyState"

export type CommentsListProps = {
  isFetching: boolean
  comments?: (Review | ProposalComment)[]
  error?: Error
  showCommentInput?: boolean
  commentInputPlaceholder?: string
  noCommentsLabel: string
  showMoreCommentsLabel: string
}

export const CommentsList = memo(
  ({
    isFetching,
    comments,
    commentInputPlaceholder,
    noCommentsLabel,
    showCommentInput,
    showMoreCommentsLabel,
  }: CommentsListProps) => (
    <div className="flex flex-col gap-4">
      {isFetching || !comments ? (
        <div>⏱️ PENDING</div>
      ) : comments.length === 0 ? (
        <>
          {showCommentInput ? (
            <CommentInput placeholder={commentInputPlaceholder} />
          ) : (
            <CommentsListEmptyState label={noCommentsLabel} />
          )}
        </>
      ) : (
        <>
          {showCommentInput && <CommentInput placeholder={commentInputPlaceholder} />}
          {comments.map(r => (
            <Comment key={r.id} text={r.text} rating={r.rating} account={r.creatorAccount} created={r.created} />
          ))}
          <ButtonOutline className="mx-auto h-9" label={showMoreCommentsLabel} />
        </>
      )}
    </div>
  ),
)
