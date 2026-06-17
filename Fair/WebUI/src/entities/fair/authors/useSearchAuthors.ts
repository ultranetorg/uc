import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useSearchAuthors = (query?: string, limit?: number) => {
  const queryFn = async () => api.searchAuthors(query, limit)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["authors", { query, limit }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
