import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { SearchDropdownItem } from "ui/components"

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

export const useSearchSites = (page?: number, search?: string) => {
  const queryFn = () => api.searchSites(page, search)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", { page, search }],
    queryFn: queryFn,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useSearchLightSites = (query?: string) => {
  const queryFn = async () => {
    const response = await api.searchLightSites(query)
    return response.items.map<SearchDropdownItem>(x => ({
      id: x.id,
      value: x.title,
    }))
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", "search", { query }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, error: error ?? undefined, data }
}
