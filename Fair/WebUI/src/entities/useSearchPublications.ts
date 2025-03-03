import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchPublications = (siteId?: string, name?: string, page?: number, pageSize?: number) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.searchPublications(siteId, name, page, pageSize)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", { name, page, pageSize }],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
