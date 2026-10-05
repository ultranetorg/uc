import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"
import { proposalsKeys } from "./proposalsKeys"

const api = getFairApi()

export const useGetProposalDetails = (storeId?: string, discussionId?: string) => {
  const queryFn = () => api.getModeratorDiscussion(storeId!, discussionId!)

  const { isFetching, error, data } = useQuery({
    queryKey: proposalsKeys.proposalDetails(storeId!, discussionId!),
    queryFn: queryFn,
    enabled: !!storeId && !!discussionId,
  })

  return { isFetching, error: error ?? undefined, data }
}
