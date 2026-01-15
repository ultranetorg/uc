import { useQuery } from "@tanstack/react-query"

import { getNodeApi } from "api"

const api = getNodeApi()

export const useGetPing = (baseUrl?: string) => {
  const queryFn = () => api.ping(baseUrl!)

  const { isLoading, data, refetch } = useQuery({
    queryKey: ["ping"],
    queryFn: queryFn,
    staleTime: Infinity,
    refetchOnWindowFocus: false,
    enabled: !!baseUrl,
  })

  return { data, isLoading, refetch }
}
