import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { useUserContext } from "app"
import { useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useResolveStoreId } from "hooks"
import { ProposalComment, ProposalCommentEdit } from "types"
import { Comment } from "ui/components/comments/Comment"
import { CommentInput } from "ui/components/comments/CommentInput"
import { ProposalCommentThreeDotsMenu } from "ui/components/specific"
import { buildCreatorAvatarUrl, buildCreatorLink, showToast } from "utils"

export type ProposalCommentItemProps = {
  comment: ProposalComment
}

export const ProposalCommentItem = memo(({ comment }: ProposalCommentItemProps) => {
  const { t } = useTranslation()
  const storeId = useResolveStoreId()
  const { user } = useUserContext()
  const { mutate } = useTransactMutationWithStatus()
  const { invalidateProposal } = useInvalidation()

  const [isEditing, setEditing] = useState(false)
  const [isSubmitting, setSubmitting] = useState(false)

  // Moderator comments are created by the user, publisher comments by one of the user's authors.
  const isOwn = !!user && (comment.creator.id === user.id || !!user.authorsIds?.includes(comment.creator.id))

  const handleEditStart = useCallback(() => setEditing(true), [])
  const handleEditCancel = useCallback(() => setEditing(false), [])

  const handleEditSubmit = useCallback(
    (text: string) => {
      setSubmitting(true)
      mutate(new ProposalCommentEdit(comment.id, text), {
        onSuccess: () => {
          invalidateProposal({ storeId: storeId!, proposalId: comment.proposalId })
          showToast(t("toast:commentUpdated"), "success")
          setEditing(false)
        },
        onError: err => showToast(err.toString(), "error"),
        onSettled: () => setSubmitting(false),
      })
    },
    [comment.id, comment.proposalId, invalidateProposal, mutate, storeId, t],
  )

  if (isEditing) {
    return (
      <CommentInput
        loading={isSubmitting}
        initialText={comment.text}
        submitLabel={t("buttons:saveChanges")}
        cancelLabel={t("buttons:cancel")}
        onCancel={handleEditCancel}
        onCommentSubmit={handleEditSubmit}
      />
    )
  }

  return (
    <Comment
      id={comment.id}
      text={comment.text}
      creatorId={comment.creator.id}
      creatorName={comment.creator.title}
      creatorAvatarSrc={buildCreatorAvatarUrl(comment.creator)}
      creatorLink={buildCreatorLink(storeId!, comment.creator)}
      created={comment.created}
      menu={isOwn ? <ProposalCommentThreeDotsMenu onEdit={handleEditStart} /> : undefined}
    />
  )
})
