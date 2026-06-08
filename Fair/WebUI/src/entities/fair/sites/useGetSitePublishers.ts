import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { sitesKeys } from "./sitesKeys"

const api = getFairApi()

export const useGetSitePublishers = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getSitePublishers(siteId!, page, pageSize, search)

  const { isFetching, error, data, refetch } = useQuery({
    queryKey: [...sitesKeys.publishers(siteId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
