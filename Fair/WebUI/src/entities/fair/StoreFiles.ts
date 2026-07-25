import { useInfiniteQuery, useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetStoreFiles = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getStoreFiles(storeId!, page, pageSize)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: ["stores", storeId, "files", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}

export const useGetStoreFilesInfinite = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = (page: number) => api.getStoreFiles(storeId!, page, pageSize)

  const { data, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading, refetch } = useInfiniteQuery(
    {
      initialPageParam: 0,
      queryKey: ["stores", storeId, "files", { page, pageSize }],
      queryFn: ({ pageParam = 0 }) => queryFn(pageParam),
      getNextPageParam: (lastPage, allPages) => {
        const loadedItems = allPages.flatMap(p => p.items).length
        return loadedItems < lastPage.totalItems ? allPages.length : undefined
      },
    },
  )

  return {
    data,
    isError,
    error: error ?? undefined,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    refetch,
  }
}
