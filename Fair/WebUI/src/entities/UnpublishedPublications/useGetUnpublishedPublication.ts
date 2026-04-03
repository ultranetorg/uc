import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUnpublishedPublication = (siteId?: string, publicationId?: string) => {
  const queryFn = () => api.getUnpublishedPublication(siteId!, publicationId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "publications", "unpublished", publicationId],
    queryFn: queryFn,
    enabled: !!siteId && !!publicationId,
  })

  return { isLoading, isFetching, isError, data }
}
