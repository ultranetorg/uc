import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPublicationDetails = (publicationId?: string) => {
  const queryFn = () => api.getPublicationDetails(publicationId!)

  const { isPending, isError, data } = useQuery({
    queryKey: ["publications", publicationId],
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isPending, isError, data }
}

export const useGetPublicationVersions = (publicationId?: string) => {
  const queryFn = () => api.getPublicationVersions(publicationId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["publications", publicationId, "versions", "latest"],
    queryFn: queryFn,
    enabled: !!publicationId,
  })
  return { isFetching, isError, data }
}

export const useGetCategoriesPublications = (siteId?: string) => {
  const queryFn = () => api.getCategoriesPublications(siteId!)

  const { isPending, isError, data } = useQuery({
    queryKey: ["sites", siteId, "categories", "publications"],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}

export const useGetAuthorPublications = (siteId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getAuthorPublications(siteId!, authorId!, page, pageSize)

  const { isPending, isError, data } = useQuery({
    queryKey: ["sites", siteId, "authors", authorId, "publications", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!authorId,
  })

  return { isPending, isError, data }
}

export const useGetCategoryPublications = (categoryId?: string, page?: number) => {
  const queryFn = () => api.getCategoryPublications(categoryId!, page)

  const { isPending, isError, data } = useQuery({
    queryKey: ["categories", categoryId, "publications", { page }],
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, isError, data }
}

export const useSearchPublications = (siteId?: string, page?: number, query?: string) => {
  const queryFn = () => api.searchPublications(siteId!, query, page)

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", { query, page }],
    queryFn: queryFn,
    enabled: !!siteId && !!query,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useSearchLitePublications = (siteId?: string, query?: string, disabled?: boolean) => {
  const queryFn = () => api.searchLitePublication(siteId!, query!)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", "search", query],
    queryFn: queryFn,
    enabled: !disabled && !!siteId && !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}

export const useGetPublicationDetailsDiff = (publicationId?: string, version?: number) => {
  const queryFn = () => api.getPublicationDetailsDiff(publicationId!, version!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["publications", publicationId, "diff", { version }],
    queryFn: queryFn,
    enabled: !!publicationId && version !== undefined,
  })

  return { isLoading, isFetching, isError, data }
}
