import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorDispute = (siteId?: string, disputeId?: string) => {
  const queryFn = () => {
    if (!siteId || !disputeId) {
      return
    }

    return api.getModeratorDispute(siteId, disputeId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "disputes", disputeId],
    queryFn: queryFn,
    enabled: !!siteId && !!disputeId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetModeratorDisputes = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getModeratorDisputes(siteId, page, pageSize, search)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "disputes", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data }
}
