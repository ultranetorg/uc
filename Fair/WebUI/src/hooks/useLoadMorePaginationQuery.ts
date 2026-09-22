import { useInfiniteQuery, type QueryKey } from "@tanstack/react-query"

interface UseLoadMorePaginationOptions<T> {
  queryKey: QueryKey
  queryFn: (page: number, pageSize: number) => Promise<T[]>
  pageSize?: number
  enabled?: boolean
}

export function useLoadMorePaginationQuery<T>({
  queryKey,
  queryFn,
  pageSize = 20,
  enabled = true,
}: UseLoadMorePaginationOptions<T>) {
  const query = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam }) => queryFn(pageParam, pageSize),
    initialPageParam: 0,
    enabled,
    getNextPageParam: (lastPage, allPages) => (lastPage.length < pageSize ? undefined : allPages.length),
  })

  const data = query.data?.pages.flat() ?? []

  return {
    data,
    hasNext: query.hasNextPage,
    fetchNext: query.fetchNextPage,
    isPending: query.isPending,
    isFetchingNext: query.isFetchingNextPage,
    isError: query.isError,
    error: query.error,
    refetch: query.refetch,
  }
}
