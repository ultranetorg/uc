import { useQuery } from "@tanstack/react-query"

import { getIccpNodeApi } from "api"

const api = getIccpNodeApi()

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
