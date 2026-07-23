import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetUserUnregistrationProposals = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUserUnregistrationProposals(storeId!, page, pageSize)

  const { isPending, isError, data, refetch } = useQuery({
    queryKey: ["stores", storeId, "proposals", "user-unregistrations", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, data, refetch }
}
