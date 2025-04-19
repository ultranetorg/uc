import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorReview = (reviewId?: string) => {
  const queryFn = () => {
    if (!reviewId) {
      return
    }

    return api.getModeratorReview(reviewId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "reviews", reviewId],
    queryFn: queryFn,
    enabled: !!reviewId,
  })

  return { isPending, error, data }
}

export const useGetModeratorReviews = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getModeratorReviews(siteId, page, pageSize, search)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "reviews", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
