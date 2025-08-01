import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetReviewProposal = (siteId?: string, reviewId?: string) => {
  const queryFn = () => api.getReviewProposal(siteId!, reviewId!)

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "reviews", reviewId],
    queryFn: queryFn,
    enabled: !!siteId && !!reviewId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetReviewProposals = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getReviewProposals(siteId, page, pageSize, search)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "reviews", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
