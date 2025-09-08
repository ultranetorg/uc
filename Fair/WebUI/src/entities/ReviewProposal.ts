import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetReviewProposals = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getReviewProposals(siteId!, page, pageSize, search)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "reviews", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, isError, data }
}
