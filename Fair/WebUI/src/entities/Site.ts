import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { MembersChangeType } from "types"

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

export const useGetSiteAuthors = (siteId?: string) => {
  const queryFn = () => api.getSiteAuthors(siteId!)

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "authors"],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, error: error ?? undefined, data }
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

export const useGetSiteMembers = (memberType: MembersChangeType, siteId?: string) => {
  const queryFn = () => (memberType === "author" ? api.getSiteAuthors(siteId!) : api.getSiteModerators(siteId!))

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, `${memberType}s`],
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
