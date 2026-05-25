import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { sitesKeys } from "./sitesKeys"

const api = getApi()

export const useGetSiteUsers = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getSiteUsers(siteId!, page, pageSize)

  const { isPending, isError, error, data } = useQuery({
    queryKey: [...sitesKeys.users(siteId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, error: error ?? undefined, data }
}
