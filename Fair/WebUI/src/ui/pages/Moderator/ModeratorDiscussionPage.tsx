import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDiscussion, useGetModeratorDiscussionComments } from "entities"

export const ModeratorDiscussionPage = () => {
  const { siteId, discussionId } = useParams()
  const { t } = useTranslation()

  const { isPending, data: discussion } = useGetModeratorDiscussion(siteId, discussionId)
  const { isPending: isCommentsPending, data: comments } = useGetModeratorDiscussionComments(siteId, discussionId)

  if (isPending || !discussion) {
    return "Loading..."
  }

  return (
    <div className="flex flex-col gap-2">
      <div>
        <div>ID</div>
        <div>{discussion.id}</div>
      </div>
      <div>
        <div>Text</div>
        <div>{discussion.text}</div>
      </div>
      <div>
        <div>Expiration</div>
        <div>{discussion.expiration}</div>
      </div>
      <div>
        <div>Votes</div>
        <div>
          <span className="text-red-500">{discussion.yesCount}</span> /{" "}
          <span className="text-green-500">{discussion.noCount}</span> /{" "}
          <span className="text-gray-500">{discussion.absCount}</span>
        </div>
      </div>
      <div>
        <div>Pros</div>
        <div>{discussion.pros.join(",")}</div>
      </div>
      <div>
        <div>Cons</div>
        <div>{discussion.cons.join(",")}</div>
      </div>
      <div>
        <div>Abs</div>
        <div>{discussion.abs.join(",")}</div>
      </div>
      <div>
        <div>Type:</div>
        <div>{t(discussion.option.$type, { ns: "votableOperations" })}</div>
      </div>
      <div>
        <div>Type:</div>
        <div>{JSON.stringify(discussion.option)}</div>
      </div>
      <h3>Comments:</h3>
      {isCommentsPending || !comments ? (
        "Loading comments..."
      ) : (
        <ul>
          {comments.items.map(comment => (
            <li className="m-2 border" key={comment.id}>
              <div>{comment.text}</div>
              <div>Author: {comment.creatorNickname || comment.creatorAddress}</div>
              <div>Created at: {comment.created}</div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
