import { keepPreviousData, useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { proposalCommentsKeys } from "./proposalCommentsKeys"

const api = getFairApi()

export const useGetProposalComments = (storeId?: string, proposalId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getProposalComments(storeId!, proposalId!, page, pageSize)

  const { isLoading, isPlaceholderData, error, data, refetch } = useQuery({
    queryKey: [...proposalCommentsKeys.all(storeId!, proposalId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId && !!proposalId,
    placeholderData: keepPreviousData,
  })

  return { isLoading: isLoading || isPlaceholderData, error: error ?? undefined, data, refetch }
}
