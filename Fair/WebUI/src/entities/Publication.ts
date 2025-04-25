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

export const useGetCategoryPublications = (categoryId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => {
    if (!categoryId) {
      return
    }

    return api.getCategoryPublications(categoryId, page, pageSize)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["categories", categoryId, "publications", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, isError, data }
}

export const useSearchPublications = (
  siteId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
  forceEnable?: boolean,
) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.searchPublications(siteId, page, pageSize, search)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "publications", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId && (!isUndefOrEmpty(search) || forceEnable === true),
  })

  return { isPending, error: error ?? undefined, data }
}
