import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { moderationReviewKeys } from "./moderationReviewKeys"

const api = getFairApi()

export const useGetReviewDeletionsProposals = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getReviewDeletionsProposals(storeId!, page, pageSize, search)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: [...moderationReviewKeys.deletions(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, isError, data, refetch }
}
