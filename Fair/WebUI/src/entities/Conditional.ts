import { useInfiniteQuery, useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { MembersChangeType } from "types"

const api = getApi()

export const useGetSiteMembers = (memberType: MembersChangeType, siteId?: string) => {
  const queryFn = () => (memberType === "author" ? api.getSiteAuthors(siteId!) : api.getSiteModerators(siteId!))

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, `${memberType}s`],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, error: error ?? undefined, data }
}

// export const useGetFiles = (siteId?: string, authorId?: string, page?: number, pageSize?: number) => {
//   const queryFn = () =>
//     !authorId ? api.getSiteFiles(siteId!, page, pageSize) : api.getAuthorFiles(siteId!, authorId, page, pageSize)

//   const { isPending, error, data, isFetching, refetch } = useQuery({
//     queryKey: !authorId
//       ? ["sites", siteId, "files", { page, pageSize }]
//       : ["sites", siteId, "authors", authorId, "files", { page, pageSize }],
//     queryFn: queryFn,
//     enabled: !!siteId && !!authorId,
//   })

//   return { isPending, error: error ?? undefined, data, isFetching, refetch }
// }

export const useGetFilesInfinite = (siteId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = (page: number) =>
    !authorId ? api.getSiteFiles(siteId!, page, pageSize) : api.getAuthorFiles(siteId!, authorId, page, pageSize)

  const { data, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading, refetch } = useInfiniteQuery(
    {
      initialPageParam: 0,
      queryKey: !authorId
        ? ["sites", siteId, "files", { page, pageSize }]
        : ["sites", siteId, "authors", authorId, "files", { page, pageSize }],
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
