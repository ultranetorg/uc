import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { sitesKeys } from "./sitesKeys"

const api = getApi()

export const useGetSite = (siteId?: string) => {
  const queryFn = () => api.getSite(siteId!)

  const { isPending, error, data } = useQuery({
    queryKey: sitesKeys.detail(siteId!),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data }
}
