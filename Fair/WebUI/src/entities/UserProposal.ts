import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUserProposals = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getUserProposals(siteId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "users", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
