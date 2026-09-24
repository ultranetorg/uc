import { useState } from "react"

import { getFairApi } from "api"
import { useLoadMorePaginationQuery } from "hooks"
import { Review, TotalItemsResult } from "types"

const api = getFairApi()

export const useGetPaginatedReviews = (publicationId?: string, pageSize?: number) => {
  const [totalItems, setTotalItems] = useState(0)

  const {
    data: items,
    hasNext,
    fetchNext,
    isPending,
    isFetchingNext,
    error,
    refetch,
  } = useLoadMorePaginationQuery<Review>({
    queryKey: ["publications", publicationId, "reviews"],
    queryFn: async (page, size) => {
      const res = await api.getReviews(publicationId!, page, size)
      setTotalItems(res.totalItems)
      return res.items
    },
    pageSize,
    enabled: !!publicationId,
  })

  const reviews: TotalItemsResult<Review> | undefined = isPending
    ? undefined
    : { items, totalItems, page: 0, pageSize: pageSize ?? items.length }

  return {
    reviews,
    isPending,
    isFetchingNext,
    error: error ?? undefined,
    hasMoreReviews: hasNext,
    fetchNextReviews: fetchNext,
    refetch,
  }
}
