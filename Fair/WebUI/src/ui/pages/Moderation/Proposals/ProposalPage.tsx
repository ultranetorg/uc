import { useParams } from "react-router-dom"

import { useGetModeratorDiscussion } from "entities"
import { ProposalView } from "ui/views"

export const ProposalPage = () => {
  const { siteId, discussionId } = useParams()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, discussionId)

  return <ProposalView isFetching={isFetching} proposal={proposal} />
}
