import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetReviewProposals = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getReviewProposals(storeId!, page, pageSize, search)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: ["moderator", "stores", storeId, "reviews", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, isError, data, refetch }
}
