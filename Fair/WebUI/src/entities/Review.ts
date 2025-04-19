import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetReviews = (publicationId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => {
    if (!publicationId) {
      return
    }

    return api.getReviews(publicationId, page, pageSize)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["publications", publicationId, "reviews", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isPending, error, data }
}
