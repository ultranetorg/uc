import { useParams } from "react-router-dom"

import { useGetPublisherProposals } from "entities"

export const PublishersProposalsTab = () => {
  const { siteId } = useParams()

  const { data: proposals } = useGetPublisherProposals(siteId)

  return <>{JSON.stringify(proposals)}</>
}
