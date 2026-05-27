import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUserUnregistrationProposals = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUserUnregistrationProposals(siteId!, page, pageSize)

  const { isPending, isError, data, refetch } = useQuery({
    queryKey: ["sites", siteId, "proposals", "user-unregistrations", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data, refetch }
}
