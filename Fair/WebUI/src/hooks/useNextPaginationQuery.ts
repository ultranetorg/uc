import { useState, useCallback, useEffect } from "react"
import { useInfiniteQuery, type QueryKey } from "@tanstack/react-query"

interface UseNextPaginationOptions<T> {
  queryKey: QueryKey
  queryFn: (pageParam: number, pageSize: number) => Promise<T[]>
  pageSize?: number
  enabled?: boolean
}

export function useNextPaginationQuery<T>({
  queryKey,
  queryFn,
  pageSize = 20,
  enabled = false,
}: UseNextPaginationOptions<T>) {
  const [page, setPage] = useState(0)
  const queryKeyStr = JSON.stringify(queryKey)

  useEffect(() => {
    setPage(0)
  }, [queryKeyStr])

  const query = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam }) => queryFn(pageParam, pageSize),
    initialPageParam: 0,
    enabled,
    getNextPageParam: (lastPage, allPages) => (!lastPage || lastPage.length < pageSize ? undefined : allPages.length),
    refetchOnWindowFocus: false,
  })

  const loadedPages = query.data?.pages ?? []
  const currentPage = loadedPages[page] ?? []
  const isOnLastLoaded = page === loadedPages.length - 1

  const hasNext = isOnLastLoaded ? query.hasNextPage && !query.isFetchingNextPage : true

  const onPageChange = useCallback(
    async (targetPage: number) => {
      if (targetPage < loadedPages.length) {
        setPage(targetPage)
        return
      }
      if (query.hasNextPage) {
        const res = await query.fetchNextPage()
        const newPages = res.data?.pages ?? []
        const last = newPages[newPages.length - 1]
        if (newPages.length > loadedPages.length && last && last.length > 0) {
          setPage(newPages.length - 1)
        }
      }
    },
    [loadedPages.length, query],
  )

  return {
    data: currentPage,
    page,
    loadedPagesCount: loadedPages.length,
    hasNext,
    onPageChange,
    isPending: query.isPending,
    isFetchingNext: query.isFetchingNextPage,
    isError: query.isError,
    error: query.error,
  }
}
