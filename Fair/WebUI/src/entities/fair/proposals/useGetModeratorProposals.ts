import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { proposalsKeys } from "./proposalsKeys"

const api = getFairApi()

export const useGetModeratorProposals = (storeId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getModeratorProposals(storeId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: [...proposalsKeys.moderators(storeId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
