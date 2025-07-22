import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorDiscussion = (siteId?: string, discussionId?: string) => {
  const queryFn = () => {
    if (!siteId || !discussionId) {
      return
    }

    return api.getModeratorDiscussion(siteId, discussionId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "discussions", discussionId],
    queryFn: queryFn,
    enabled: !!siteId && !!discussionId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetModeratorDiscussions = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getModeratorDiscussions(siteId, page, pageSize, search)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "discussions", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data }
}
