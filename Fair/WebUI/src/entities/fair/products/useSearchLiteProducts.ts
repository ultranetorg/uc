import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchLiteProducts = (query?: string) => {
  const queryFn = () => api.searchLiteProducts(query)

  const { isPending, error, data } = useQuery({
    queryKey: ["products", "search", { query }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, error: error ?? undefined, data }
}
