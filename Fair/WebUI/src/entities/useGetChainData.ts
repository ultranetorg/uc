import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { DASHBOARD_REFETCH_INTERVAL } from "config"

export const useGetChainData = () => {
  const api = getApi()

  const queryFn = async () => {
    return await api.chain.get()
  }

  const { data, isLoading } = useQuery({
    queryKey: ["chain", "details"],
    queryFn: queryFn,
    refetchInterval: DASHBOARD_REFETCH_INTERVAL,
  })

  return {
    data,
    isLoading,
  }
}
