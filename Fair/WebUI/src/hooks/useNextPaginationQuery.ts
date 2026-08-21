import { useState, useCallback, useEffect } from "react"
import { useInfiniteQuery, useQueryClient, type InfiniteData, type QueryKey } from "@tanstack/react-query"

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
  // Set once fetchNextPage comes back empty, so a total that's an exact multiple of pageSize
  // (which makes the last real page look "full" to getNextPageParam) doesn't re-offer a next page.
  const [exhausted, setExhausted] = useState(false)
  const queryKeyStr = JSON.stringify(queryKey)
  const queryClient = useQueryClient()

  useEffect(() => {
    setPage(0)
    setExhausted(false)
  }, [queryKeyStr])

  const query = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam }) => queryFn(pageParam, pageSize),
    initialPageParam: 0,
    enabled,
    getNextPageParam: (lastPage, allPages) => (!lastPage || lastPage.length < pageSize ? undefined : allPages.length),
  })

  const loadedPages = query.data?.pages ?? []
  const currentPage = loadedPages[page] ?? []
  const isOnLastLoaded = page === loadedPages.length - 1

  const hasNext = isOnLastLoaded ? query.hasNextPage && !exhausted && !query.isFetchingNextPage : true

  const onPageChange = useCallback(
    async (targetPage: number) => {
      if (targetPage < loadedPages.length) {
        setPage(targetPage)
        return
      }
      if (!query.hasNextPage || exhausted) {
        return
      }
      const res = await query.fetchNextPage()
      const newPages = res.data?.pages ?? []
      const last = newPages[newPages.length - 1]
      if (newPages.length > loadedPages.length && last && last.length > 0) {
        setPage(newPages.length - 1)
        return
      }
      // The fetched page turned out empty: there's nothing beyond what's already loaded.
      // Drop it from the cache and remember that, so getNextPageParam re-checking the
      // previous (exactly full) page doesn't keep offering a next page that isn't there.
      setExhausted(true)
      queryClient.setQueryData<InfiniteData<T[], number>>(queryKey, old =>
        old ? { pages: old.pages.slice(0, -1), pageParams: old.pageParams.slice(0, -1) } : old,
      )
    },
    [loadedPages.length, query, exhausted, queryClient, queryKey],
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
