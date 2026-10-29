import { memo } from "react"

import { ProposalComment, Review } from "types"
import { useResolveStoreId } from "hooks"
import { buildCreatorAvatarUrl, buildCreatorLink } from "utils"

import { Comment } from "./Comment"
import { CommentInput } from "./CommentInput"
import { CommentsListEmptyState } from "./CommentsListEmptyState"

export type CommentsListProps = {
  inputDisabled?: boolean
  inputLoading?: boolean
  isFetching: boolean
  comments?: (Review | ProposalComment)[]
  error?: Error
  showCommentInput?: boolean
  commentInputPlaceholder?: string
  noCommentsLabel: string
  showMoreCommentsLabel: string
  onCommentSubmit: (comment: string) => void
}

export const CommentsList = memo(
  ({
    inputDisabled,
    inputLoading,
    isFetching,
    comments,
    commentInputPlaceholder,
    noCommentsLabel,
    showCommentInput,
    onCommentSubmit,
  }: CommentsListProps) => {
    const storeId = useResolveStoreId()

    return (
      <div className="flex flex-col gap-4">
        {isFetching || !comments ? (
          <div>Loading{import.meta.env.DEV ? " (CommentsList)" : ""}</div>
        ) : comments.length === 0 ? (
          <>
            {showCommentInput ? (
              <CommentInput
                disabled={inputDisabled}
                loading={inputLoading}
                placeholder={commentInputPlaceholder}
                onCommentSubmit={onCommentSubmit}
              />
            ) : (
              <CommentsListEmptyState label={noCommentsLabel} />
            )}
          </>
        ) : (
          <>
            {showCommentInput && (
              <CommentInput
                disabled={inputDisabled}
                loading={inputLoading}
                placeholder={commentInputPlaceholder}
                onCommentSubmit={onCommentSubmit}
              />
            )}
            {comments.map(r => (
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
            ))}
            {/* <ButtonOutline className="mx-auto h-9" label={showMoreCommentsLabel} /> */}
          </>
        )}
      </div>
    )
  },
)
