import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorDiscussionComments = (
  siteId?: string,
  discussionId?: string,
  page?: number,
  pageSize?: number,
) => {
  const queryFn = () => api.getModeratorDiscussionComments(siteId!, discussionId!, page, pageSize)

  const { isFetching, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "discussions", discussionId, "comments", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!discussionId,
  })

  return { isFetching, error: error ?? undefined, data }
}
