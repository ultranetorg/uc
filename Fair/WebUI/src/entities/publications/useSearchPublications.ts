import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchPublications = (siteId?: string, page?: number, query?: string) => {
  const queryFn = () => api.searchPublications(siteId!, query, page)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", { query, page }],
    queryFn: queryFn,
    enabled: !!siteId && !!query,
  })

  return { isPending, error: error ?? undefined, data }
}
