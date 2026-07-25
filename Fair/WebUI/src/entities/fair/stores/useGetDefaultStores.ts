import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetDefaultStores = (enabled?: boolean) => {
  const queryFn = () => api.getDefaultStores()

  const { isFetching, error, data } = useQuery({
    queryKey: ["stores", "default"],
    queryFn: queryFn,
    enabled: !!enabled,
  })

  return { isFetching, error: error ?? undefined, data }
}
