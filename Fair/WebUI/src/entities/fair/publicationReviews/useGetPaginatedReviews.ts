import { useState } from "react"

import { getFairApi } from "api"
import { useLoadMorePaginationQuery } from "hooks"
import { Review, TotalItemsResult } from "types"
import { reviewKeys } from "./reviewKeys"

const api = getFairApi()

export const useGetPaginatedReviews = (publicationId?: string, pageSize?: number) => {
  const [totalItems, setTotalItems] = useState(0)

  const {
    data: items,
    hasNext,
    fetchNext,
    isLoading,
    isFetchingNext,
    error,
    refetch,
  } = useLoadMorePaginationQuery<Review>({
    queryKey: reviewKeys.all(publicationId!),
    queryFn: async (page, size) => {
      const res = await api.getReviews(publicationId!, page, size)
      setTotalItems(res.totalItems)
      return res.items
    },
    pageSize,
    enabled: !!publicationId,
  })

  const reviews: TotalItemsResult<Review> | undefined = isLoading
    ? undefined
    : { items, totalItems, page: 0, pageSize: pageSize ?? items.length }

  return {
    reviews,
    isLoading,
    isFetchingNext,
    error: error ?? undefined,
    hasMoreReviews: hasNext,
    fetchNextReviews: fetchNext,
    refetch,
  }
}
