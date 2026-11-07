import { useQuery } from "@tanstack/react-query"

import { getIccpNodeApi } from "api"

const api = getIccpNodeApi()

export const useGetPing = (iccpNodeUrl?: string, refetchInterval?: number | false) => {
  const queryFn = () => api.ping(iccpNodeUrl!)

  const { isLoading, data, refetch } = useQuery({
    queryKey: ["ping"],
    queryFn: queryFn,
    enabled: !!iccpNodeUrl,
    // Явный опрос: интервал включается вызывающим кодом, по умолчанию выключен.
    refetchInterval: refetchInterval,
  })

  return { data, isLoading, refetch }
}
