import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorDiscussionComments = (
  siteId?: string,
  discussionId?: string,
  page?: number,
  pageSize?: number,
) => {
  const queryFn = () => {
    if (!siteId || !discussionId) {
      return
    }

    return api.getModeratorDiscussionComments(siteId, discussionId, page, pageSize)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "discussions", discussionId, "comments", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!discussionId,
  })

  return { isPending, error: error ?? undefined, data }
}
