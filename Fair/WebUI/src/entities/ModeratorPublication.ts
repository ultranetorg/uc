import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetModeratorPublication = (publicationId?: string) => {
  const queryFn = () => {
    if (!publicationId) {
      return
    }

    return api.getModeratorPublication(publicationId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "publications", publicationId],
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetModeratorPublications = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getModeratorPublications(siteId, page, pageSize, search)
  }

  const { isPending, isError, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "publications", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
