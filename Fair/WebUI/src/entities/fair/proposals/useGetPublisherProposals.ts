import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { proposalsKeys } from "./proposalsKeys"

const api = getFairApi()

export const useGetPublisherProposals = (storeId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getPublisherProposals(storeId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: [...proposalsKeys.publishers(storeId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
