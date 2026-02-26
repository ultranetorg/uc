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

export const useGetSitePolicies = (siteId?: string) => {
  const { isFetching, error, data, refetch } = useQuery({
    queryKey: ["sites", siteId, "policies"],
    queryFn: () => api.getSitePolicies(siteId!),
    enabled: !!siteId,
    // Auto refetch disable
    staleTime: Infinity,
    refetchOnMount: false,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}

export const useGetSiteModerators = (siteId?: string) => {
  const queryFn = () => api.getSiteModerators(siteId!)

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "moderators"],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, error: error ?? undefined, data }
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

export const useSearchLiteSites = (query?: string) => {
  const queryFn = async () => api.searchLiteSites(query)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", "search", query],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
