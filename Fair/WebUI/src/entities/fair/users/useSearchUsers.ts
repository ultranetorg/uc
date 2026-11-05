import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchUsers = (query?: string, limit?: number) => {
  const queryFn = async () => api.searchUsers(query, limit)

  const { isLoading, isFetching, error, data } = useQuery({
    queryKey: ["users", { query, limit }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isLoading, isFetching, error: error ?? undefined, data }
}
