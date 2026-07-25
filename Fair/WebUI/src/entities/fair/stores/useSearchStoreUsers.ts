import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchStoreUsers = (storeId: string, query?: string) => {
  const queryFn = async () => api.searchStoreUsers(storeId, query)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["stores", storeId, "users", "search", { query }],
    queryFn: queryFn,
    enabled: !!storeId && !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
