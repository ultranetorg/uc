import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchAccounts = (query?: string, limit?: number) => {
  const queryFn = async () => api.searchAccounts(query, limit)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["accounts", { query, limit }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
