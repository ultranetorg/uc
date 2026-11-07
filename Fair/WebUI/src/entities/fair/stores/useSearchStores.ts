import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchStores = (query?: string, page?: number) => {
  const queryFn = () => api.searchStores(query, page)

  const { isLoading, error, data } = useQuery({
    queryKey: ["stores", { page, query }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isLoading, error: error ?? undefined, data }
}
