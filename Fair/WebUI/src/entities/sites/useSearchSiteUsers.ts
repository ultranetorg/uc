import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchSiteUsers = (siteId: string, query?: string) => {
  const queryFn = async () => api.searchSiteUsers(siteId, query)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "users", "search", { query }],
    queryFn: queryFn,
    enabled: !!siteId && !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
