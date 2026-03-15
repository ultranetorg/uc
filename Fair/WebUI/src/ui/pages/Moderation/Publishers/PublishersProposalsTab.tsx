import { useParams } from "react-router-dom"

import { useGetModeratorProposals } from "entities"

export const PublishersProposalsTab = () => {
  const { siteId } = useParams()

  const { data: proposals } = useGetModeratorProposals(siteId)

  return <>{JSON.stringify(proposals)}</>
}
