import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDiscussion, useGetModeratorDiscussionComments } from "entities"
import { ProposalView } from "ui/views"

export const ModeratorReviewPage = () => {
  const { t } = useTranslation()
  const { siteId, discussionId } = useParams()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, discussionId)
  const { isFetching: isCommentsFetching, data: comments } = useGetModeratorDiscussionComments(siteId, proposal?.id)

  return (
    <ProposalView
      parentBreadcrumb={{ title: t("common:moderation"), path: `/${siteId}/m/` }}
      isFetching={isFetching}
      proposal={proposal}
      isCommentsFetching={isCommentsFetching}
      comments={comments}
    />
  )
}
