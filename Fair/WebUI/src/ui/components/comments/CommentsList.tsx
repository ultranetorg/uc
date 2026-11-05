import { Fragment, memo, ReactNode } from "react"

import { ProposalComment, Review } from "types"
import { useResolveStoreId } from "hooks"
import { Spinner } from "ui/components"
import { buildCreatorAvatarUrl, buildCreatorLink } from "utils"

import { Comment } from "./Comment"
import { CommentInput } from "./CommentInput"
import { CommentsListEmptyState } from "./CommentsListEmptyState"

export type CommentsListProps = {
  inputDisabled?: boolean
  inputLoading?: boolean
  isPending: boolean
  comments?: (Review | ProposalComment)[]
  error?: Error
  showCommentInput?: boolean
  commentInputPlaceholder?: string
  noCommentsLabel: string
  showMoreCommentsLabel: string
  renderComment?: (comment: Review | ProposalComment) => ReactNode
  onCommentSubmit: (comment: string) => void
}

export const CommentsList = memo(
  ({
    inputDisabled,
    inputLoading,
    isPending,
    comments,
    commentInputPlaceholder,
    noCommentsLabel,
    showCommentInput,
    renderComment,
    onCommentSubmit,
  }: CommentsListProps) => {
    const storeId = useResolveStoreId()

    return (
      <div className="flex flex-col gap-4">
        {showCommentInput && (
          <CommentInput
            disabled={inputDisabled}
            loading={inputLoading}
            placeholder={commentInputPlaceholder}
            onCommentSubmit={onCommentSubmit}
          />
        )}
        {isPending || !comments ? (
          <Spinner />
        ) : comments.length === 0 ? (
          !showCommentInput && <CommentsListEmptyState label={noCommentsLabel} />
        ) : (
          comments.map(r =>
            renderComment ? (
              <Fragment key={r.id}>{renderComment(r)}</Fragment>
            ) : (
              <Comment
                key={r.id}
                id={r.id}
                text={r.text}
                rating={"rating" in r ? r.rating : undefined}
                creatorId={r.creator.id}
                creatorName={r.creator.title}
                creatorAvatarSrc={buildCreatorAvatarUrl(r.creator)}
                creatorLink={buildCreatorLink(storeId!, r.creator)}
                created={r.created}
              />
            ),
          )
        )}
      </div>
    )
  },
)
