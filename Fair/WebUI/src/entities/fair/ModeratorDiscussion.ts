import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetModeratorDiscussion = (storeId?: string, discussionId?: string) => {
  const queryFn = () => api.getModeratorDiscussion(storeId!, discussionId!)

  const { isFetching, error, data } = useQuery({
    queryKey: ["moderator", "stores", storeId, "discussions", discussionId],
    queryFn: queryFn,
    enabled: !!storeId && !!discussionId,
  })

  return { isFetching, error: error ?? undefined, data }
}

export const useGetModeratorDiscussions = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getModeratorDiscussions(storeId!, page, pageSize, search)

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "stores", storeId, "discussions", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, error: error ?? undefined, data }
}
