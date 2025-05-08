import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { isUndefOrEmpty } from "utils"

const api = getApi()

export const useGetPublication = (publicationId?: string) => {
  const queryFn = () => {
    if (!publicationId) {
      return
    }

    return api.getPublication(publicationId)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["publications", publicationId],
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isPending, isError, data }
}

export const useGetCategoriesPublications = (siteId?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getCategoriesPublications(siteId)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["sites", siteId, "categories", "publications"],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}

export const useGetAuthorPublications = (siteId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => {
    if (!siteId || !authorId) {
      return
    }

    return api.getAuthorPublications(siteId, authorId, page, pageSize)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["sites", siteId, "authors", authorId, "publications", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!authorId,
  })

  return { isPending, isError, data }
}

export const useGetCategoryPublications = (categoryId?: string, page?: number) => {
  const queryFn = () => {
    if (!categoryId) {
      return
    }

    return api.getCategoryPublications(categoryId, page)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["categories", categoryId, "publications", { page }],
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, isError, data }
}

export const useSearchPublications = (siteId?: string, page?: number, query?: string, forceEnable?: boolean) => {
  const queryFn = () => api.searchPublications(siteId!, query, page)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", { query, page }],
    queryFn: queryFn,
    enabled: !!siteId && (!isUndefOrEmpty(query) || forceEnable === true),
  })

  return { isPending, error: error ?? undefined, data }
}

export const useSearchLitePublications = (siteId?: string, query?: string) => {
  const queryFn = () => api.searchLitePublication(siteId!, query!)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", "search", query],
    queryFn: queryFn,
    enabled: !!siteId && !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
