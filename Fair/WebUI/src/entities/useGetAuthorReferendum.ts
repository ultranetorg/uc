import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetAuthorReferendum = (siteId?: string, referendumId?: string) => {
  const queryFn = () => {
    if (!siteId || !referendumId) {
      return
    }

    return api.getAuthorReferendum(siteId, referendumId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["author", "sites", siteId, "referendums", referendumId],
    queryFn: queryFn,
    enabled: !!siteId && !!referendumId,
  })

  return { isPending, error, data }
}

export const useGetAuthorReferendums = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getAuthorReferendums(siteId, page, pageSize, search)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["author", "sites", siteId, "referendums", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error, data }
}
