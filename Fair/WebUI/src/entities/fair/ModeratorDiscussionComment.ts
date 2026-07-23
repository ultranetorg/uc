import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetModeratorDiscussionComments = (
  storeId?: string,
  discussionId?: string,
  page?: number,
  pageSize?: number,
) => {
  const queryFn = () => api.getModeratorDiscussionComments(storeId!, discussionId!, page, pageSize)

  const { isFetching, error, data, refetch } = useQuery({
    queryKey: ["moderator", "stores", storeId, "discussions", discussionId, "comments", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId && !!discussionId,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
