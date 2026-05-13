import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { sitesKeys } from "./sitesKeys"

const api = getApi()

export const useGetSiteModerators = (siteId?: string) => {
  const queryFn = () => api.getSiteModerators(siteId!)

  const { isFetching, error, data, refetch } = useQuery({
    queryKey: sitesKeys.moderators(siteId!),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
