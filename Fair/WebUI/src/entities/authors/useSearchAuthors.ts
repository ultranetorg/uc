import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchAuthors = (query?: string, limit?: number) => {
  const queryFn = async () => api.searchAuthors(query, limit)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["authors", { query, limit }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
