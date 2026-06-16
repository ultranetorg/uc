import { useQuery } from "@tanstack/react-query"

import { getIccpNodeApi } from "api"

const api = getIccpNodeApi()

export const useGetPing = (iccpNodeUrl?: string, refetchInterval?: number | false) => {
  const queryFn = () => api.ping(iccpNodeUrl!)

  const { isPending, data, refetch } = useQuery({
    queryKey: ["ping"],
    queryFn: queryFn,
    staleTime: Infinity,
    refetchOnWindowFocus: false,
    enabled: !!iccpNodeUrl,
    refetchInterval: refetchInterval,
  })

  return { data, isPending, refetch }
}
