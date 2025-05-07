import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetDefaultSites = (enabled?: boolean) => {
  const queryFn = () => api.getDefaultSites()

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", "default"],
    queryFn: queryFn,
    enabled: !!enabled,
  })

  return { isFetching, error: error ?? undefined, data }
}

export const useGetSite = (siteId?: string) => {
  const queryFn = () => api.getSite(siteId!)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useSearchSites = (query?: string, page?: number) => {
  const queryFn = () => api.searchSites(query, page)

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", { page, query }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isFetching, error: error ?? undefined, data }
}

export const useSearchLightSites = (query?: string) => {
  const queryFn = async () => api.searchLightSites(query)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", "search", query],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, error: error ?? undefined, data }
}
