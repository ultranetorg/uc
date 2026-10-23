import { useInfiniteQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storeFilesKeys } from "./storeFilesKeys"

const api = getFairApi()

export const useGetStoreFilesInfinite = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = (page: number) => api.getStoreFiles(storeId!, page, pageSize)

  const { data, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading, refetch } = useInfiniteQuery(
    {
      initialPageParam: 0,
      queryKey: storeFilesKeys.infinite(storeId!, page, pageSize),
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
