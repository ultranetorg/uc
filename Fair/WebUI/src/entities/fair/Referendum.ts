import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetAuthorReferendum = (storeId?: string, referendumId?: string) => {
  const queryFn = () => api.getAuthorReferendum(storeId!, referendumId!)

  const { isFetching, error, data } = useQuery({
    queryKey: ["author", "stores", storeId, "referendums", referendumId],
    queryFn: queryFn,
    enabled: !!storeId && !!referendumId,
  })

  return { isFetching, error: error ?? undefined, data }
}

export const useGetAuthorReferendums = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getAuthorReferendums(storeId!, page, pageSize, search)

  const { isFetching, error, data } = useQuery({
    queryKey: ["author", "stores", storeId, "referendums", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, error: error ?? undefined, data }
}
