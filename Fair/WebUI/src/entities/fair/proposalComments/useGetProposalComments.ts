import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"
import { proposalCommentsKeys } from "./proposalCommentsKeys"

const api = getFairApi()

export const useGetProposalComments = (storeId?: string, discussionId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getModeratorDiscussionComments(storeId!, discussionId!, page, pageSize)

  const { isFetching, error, data, refetch } = useQuery({
    queryKey: [...proposalCommentsKeys.all(storeId!, discussionId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId && !!discussionId,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
