import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchPublications = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.searchPublications(siteId, page, pageSize, search)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error, data }
}
