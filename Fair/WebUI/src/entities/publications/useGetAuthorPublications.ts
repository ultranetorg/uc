import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetAuthorPublications = (siteId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getAuthorPublications(siteId!, authorId!, page, pageSize)

  const { isPending, isError, data } = useQuery({
    queryKey: ["sites", siteId, "authors", authorId, "publications", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!authorId,
  })

  return { isPending, isError, data }
}
