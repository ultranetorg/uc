import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchPublications = (siteId?: string, page?: number, pageSize?: number, title?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.searchPublications(siteId, page, pageSize, title)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", { page, pageSize, title }],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
