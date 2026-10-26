import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { referendumsKeys } from "./referendumsKeys"

const api = getFairApi()

export const useGetReferendums = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getReferendums(storeId!, page, pageSize, search)

  const { isFetching, error, data } = useQuery({
    queryKey: [...referendumsKeys.all(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, error: error ?? undefined, data }
}
