import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetChangedPublication = (siteId?: string, changedPublicationId?: string) => {
  const queryFn = () => api.getChangedPublication(siteId!, changedPublicationId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "publications", "changed", changedPublicationId],
    queryFn: queryFn,
    enabled: !!siteId && !!changedPublicationId,
  })

  return { isLoading, isFetching, isError, data }
}

export const useGetChangedPublications = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getChangedPublications(siteId!, page, pageSize)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "publications", "changed", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })
  return { isFetching, isError, data }
}
