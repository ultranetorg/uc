import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchLiteStores = (query?: string) => {
  const queryFn = async () => api.searchLiteStores(query)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["stores", "search", query],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
