import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPublicationDetailsDiff = (publicationId?: string, version?: number) => {
  const queryFn = () => api.getPublicationDetailsDiff(publicationId!, version!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["publications", publicationId, "diff", { version }],
    queryFn: queryFn,
    enabled: !!publicationId && version !== undefined,
  })

  return { isLoading, isFetching, isError, data }
}
