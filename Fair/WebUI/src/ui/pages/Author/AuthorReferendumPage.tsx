import { useParams } from "react-router-dom"

import { useGetAuthorReferendum } from "entities"

export const AuthorReferendumPage = () => {
  const { siteId, referendumId } = useParams()
  const { isPending, data: referendum } = useGetAuthorReferendum(siteId, referendumId)

  if (isPending) {
    return "Loading..."
  }

  return (
    <div className="flex flex-col gap-2">
      <div>
        <div>ID</div>
        <div>{referendum?.id}</div>
      </div>
      <div>
        <div>Flags</div>
        <div>{referendum?.flags}</div>
      </div>
      <div>
        <div>Proposal</div>
        <div>{JSON.stringify(referendum?.proposal)}</div>
      </div>
      <div>
        <div>Pros</div>
        <div>{referendum?.pros.join(",")}</div>
      </div>
      <div>
        <div>Cons</div>
        <div>{referendum?.cons.join(",")}</div>
      </div>
      <div>
        <div>Expiration</div>
        <div>{referendum?.expiration}</div>
      </div>
    </div>
  )
}
