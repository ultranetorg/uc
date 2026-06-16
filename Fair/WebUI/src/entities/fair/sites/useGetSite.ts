import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { sitesKeys } from "./sitesKeys"

const api = getFairApi()

export const useGetSite = (siteId?: string) => {
  const queryFn = () => api.getSite(siteId!)

  const { isPending, error, data } = useQuery({
    queryKey: sitesKeys.detail(siteId!),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data }
}
