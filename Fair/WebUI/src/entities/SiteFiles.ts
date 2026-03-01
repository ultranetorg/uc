import { useInfiniteQuery, useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSiteFiles = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getSiteFiles(siteId!, page, pageSize)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: ["sites", siteId, "files", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}

export const useGetSiteFilesInfinite = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = (page: number) => api.getSiteFiles(siteId!, page, pageSize)

  const { data, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading, refetch } = useInfiniteQuery(
    {
      initialPageParam: 0,
      queryKey: ["sites", siteId, "files", { page, pageSize }],
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
