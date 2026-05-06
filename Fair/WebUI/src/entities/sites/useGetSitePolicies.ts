import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSitePolicies = (enabled: boolean, siteId?: string) => {
  const { isFetching, error, data, refetch } = useQuery({
    queryKey: ["sites", siteId, "policies"],
    queryFn: () => api.getSitePolicies(siteId!),
    enabled: !!siteId && enabled === true,
    // Auto refetch disable
    staleTime: Infinity,
    refetchOnMount: false,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
