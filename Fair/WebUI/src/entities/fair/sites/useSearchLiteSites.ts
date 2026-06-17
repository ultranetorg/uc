import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchLiteSites = (query?: string) => {
  const queryFn = async () => api.searchLiteSites(query)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", "search", query],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
