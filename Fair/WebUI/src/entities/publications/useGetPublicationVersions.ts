import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPublicationVersions = (publicationId?: string) => {
  const queryFn = () => api.getPublicationVersions(publicationId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["publications", publicationId, "versions", "latest"],
    queryFn: queryFn,
    enabled: !!publicationId,
  })
  return { isFetching, isError, data }
}
