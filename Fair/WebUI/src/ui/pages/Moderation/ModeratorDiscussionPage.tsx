import { useParams } from "react-router-dom"

import { useGetModeratorDiscussion, useGetModeratorDiscussionComments } from "entities"

export const ModeratorDiscussionPage = () => {
  const { siteId, discussionId } = useParams()

  const { isPending, data: discussion } = useGetModeratorDiscussion(siteId, discussionId)
  const { isPending: isCommentsPending, data: comments } = useGetModeratorDiscussionComments(siteId, discussionId)

  if (isPending || !discussion || isCommentsPending) {
    return "Loading..."
  }

  return (
    <div className="flex flex-col gap-2">
      DISCUSSION
      <div>{JSON.stringify(discussion)}</div>
      COMMENTS:
      <div>{JSON.stringify(comments)}</div>
    </div>
  )
}
