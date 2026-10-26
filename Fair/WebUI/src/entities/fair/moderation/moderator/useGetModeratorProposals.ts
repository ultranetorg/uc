import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { moderatorProposalsKeys } from "./moderatorProposalsKeys"

const api = getFairApi()

export const useGetModeratorProposals = (storeId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getModeratorProposals(storeId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: [...moderatorProposalsKeys.all(storeId!), { search, page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
