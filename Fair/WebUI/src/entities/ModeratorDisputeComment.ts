import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorDisputeComments = (
  siteId?: string,
  disputeId?: string,
  page?: number,
  pageSize?: number,
) => {
  const queryFn = () => {
    if (!siteId || !disputeId) {
      return
    }

    return api.getModeratorDisputeComments(siteId, disputeId, page, pageSize)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "disputes", disputeId, "comments", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!disputeId,
  })

  return { isPending, error: error ?? undefined, data }
}
