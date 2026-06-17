import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchSites = (query?: string, page?: number) => {
  const queryFn = () => api.searchSites(query, page)

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", { page, query }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isFetching, error: error ?? undefined, data }
}
