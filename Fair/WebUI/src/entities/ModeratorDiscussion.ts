import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorDiscussion = (siteId?: string, discussionId?: string) => {
  const queryFn = () => api.getModeratorDiscussion(siteId!, discussionId!)

  const { isFetching, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "discussions", discussionId],
    queryFn: queryFn,
    enabled: !!siteId && !!discussionId,
  })

  return { isFetching, error: error ?? undefined, data }
}

export const useGetModeratorDiscussions = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getModeratorDiscussions(siteId!, page, pageSize, search)

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "discussions", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetProductFields = (productId: string) => {
  const queryFn = () => api.getProductFields(productId)

  const { isPending, error, data } = useQuery({
    queryKey: ["products", productId, "fields"],
    queryFn: queryFn,
  })

  return { isPending, error: error ?? undefined, data }
}
