import { useQuery } from "@tanstack/react-query"

import { getNodeApi } from "api"

const api = getNodeApi()

export const useGetNodePing = () => {
  const queryFn = () => api.ping()

  const { isLoading, data, refetch } = useQuery({
    queryKey: ["ping"],
    queryFn: queryFn,
    staleTime: Infinity,
    refetchOnWindowFocus: false,
  })

  return { data, isLoading, refetch }
}
