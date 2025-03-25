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

  return { isPending, error, data }
}

export const useGetSites = (page?: number, pageSize?: number, title?: string) => {
  const queryFn = () => api.getSites(page, pageSize, title)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", { page, pageSize, title }],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
