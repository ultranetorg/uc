import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetAuthorFiles = (siteId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getAuthorFiles(siteId!, authorId, page, pageSize)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: ["sites", siteId, "authors", authorId, "files", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!authorId,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
