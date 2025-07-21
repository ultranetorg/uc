import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDispute, useGetModeratorDisputeComments } from "entities"

export const ModeratorDisputePage = () => {
  const { siteId, disputeId } = useParams()
  const { t } = useTranslation()

  const { isPending, data: dispute } = useGetModeratorDispute(siteId, disputeId)
  const { isPending: isCommentsPending, data: comments } = useGetModeratorDisputeComments(siteId, disputeId)

  if (isPending || !dispute) {
    return "Loading..."
  }

  return (
    <div className="flex flex-col gap-2">
      <div>
        <div>ID</div>
        <div>{dispute.id}</div>
      </div>
      <div>
        <div>Text</div>
        <div>{dispute.text}</div>
      </div>
      <div>
        <div>Expiration</div>
        <div>{dispute.expiration}</div>
      </div>
      <div>
        <div>Votes</div>
        <div>
          <span className="text-red-500">{dispute.yesCount}</span> /{" "}
          <span className="text-green-500">{dispute.noCount}</span> /{" "}
          <span className="text-gray-500">{dispute.absCount}</span>
        </div>
      </div>
      <div>
        <div>Pros</div>
        <div>{dispute.pros.join(",")}</div>
      </div>
      <div>
        <div>Cons</div>
        <div>{dispute.cons.join(",")}</div>
      </div>
      <div>
        <div>Abs</div>
        <div>{dispute.abs.join(",")}</div>
      </div>
      <div>
        <div>Type:</div>
        <div>{t(dispute.option.$type, { ns: "votableOperations" })}</div>
      </div>
      <div>
        <div>Type:</div>
        <div>{JSON.stringify(dispute.option)}</div>
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
