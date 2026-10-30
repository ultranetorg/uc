import { keepPreviousData, useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { proposalCommentsKeys } from "./proposalCommentsKeys"

const api = getFairApi()

export const useGetProposalComments = (storeId?: string, proposalId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getProposalComments(storeId!, proposalId!, page, pageSize)

  const { isFetching, error, data, refetch } = useQuery({
    queryKey: [...proposalCommentsKeys.all(storeId!, proposalId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId && !!proposalId,
    placeholderData: keepPreviousData,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
