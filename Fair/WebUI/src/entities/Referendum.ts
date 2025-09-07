import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetAuthorReferendum = (siteId?: string, referendumId?: string) => {
  const queryFn = () => api.getAuthorReferendum(siteId!, referendumId!)

  const { isFetching, error, data } = useQuery({
    queryKey: ["author", "sites", siteId, "referendums", referendumId],
    queryFn: queryFn,
    enabled: !!siteId && !!referendumId,
  })

  return { isFetching, error: error ?? undefined, data }
}

export const useGetAuthorReferendums = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getAuthorReferendums(siteId!, page, pageSize, search)

  const { isFetching, error, data } = useQuery({
    queryKey: ["author", "sites", siteId, "referendums", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, error: error ?? undefined, data }
}
