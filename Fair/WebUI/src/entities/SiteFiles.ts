import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSiteFiles = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getSiteFiles(siteId!, page, pageSize)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: ["sites", siteId, "files", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
