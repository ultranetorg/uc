import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSite = (siteId?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getSite(siteId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId],
    queryFn: queryFn,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetSites = (page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getSites(page, pageSize, search)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", { page, pageSize, search }],
    queryFn: queryFn,
  })

  return { isPending, error: error ?? undefined, data }
}
