import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchLitePublications = (storeId?: string, query?: string, disabled?: boolean) => {
  const queryFn = () => api.searchLitePublication(storeId!, query!)

  const { isLoading, isFetching, error, data } = useQuery({
    queryKey: ["stores", storeId, "publications", "search", query],
    queryFn: queryFn,
    enabled: !disabled && !!storeId && !!query,
  })

  return { isLoading, isFetching, error: error ?? undefined, data }
}
