import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetStorePolicies = (enabled: boolean, storeId?: string) => {
  const { isFetching, error, data, refetch } = useQuery({
    queryKey: ["stores", storeId, "policies"],
    queryFn: () => api.getStorePolicies(storeId!),
    enabled: !!storeId && enabled === true,
    // Auto refetch disable
    staleTime: Infinity,
    refetchOnMount: false,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
