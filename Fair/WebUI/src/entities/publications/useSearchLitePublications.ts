import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchLitePublications = (siteId?: string, query?: string, disabled?: boolean) => {
  const queryFn = () => api.searchLitePublication(siteId!, query!)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", "search", query],
    queryFn: queryFn,
    enabled: !disabled && !!siteId && !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
