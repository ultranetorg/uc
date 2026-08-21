import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchStores = (query?: string, page?: number) => {
  const queryFn = () => api.searchStores(query, page)

  const { isPending, error, data } = useQuery({
    queryKey: ["stores", { page, query }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, error: error ?? undefined, data }
}
