import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorProposals = (siteId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getModeratorProposals(siteId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "proposals", "moderator", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
