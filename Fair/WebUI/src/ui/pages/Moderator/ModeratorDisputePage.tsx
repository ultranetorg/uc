import { useParams } from "react-router-dom"

import { useGetModeratorDispute } from "entities"

export const ModeratorDisputePage = () => {
  const { siteId, disputeId } = useParams()
  const { isPending, data: dispute } = useGetModeratorDispute(siteId, disputeId)

  if (isPending) {
    return "Loading..."
  }

  return (
    <div className="flex flex-col gap-2">
      <div>
        <div>ID</div>
        <div>{dispute?.id}</div>
      </div>
      <div>
        <div>Flags</div>
        <div>{dispute?.flags}</div>
      </div>
      <div>
        <div>Proposal</div>
        <div>{JSON.stringify(dispute?.proposal)}</div>
      </div>
      <div>
        <div>Pros</div>
        <div>{dispute?.pros.join(",")}</div>
      </div>
      <div>
        <div>Cons</div>
        <div>{dispute?.cons.join(",")}</div>
      </div>
      <div>
        <div>Expiration</div>
        <div>{dispute?.expiration}</div>
      </div>
    </div>
  )
}
