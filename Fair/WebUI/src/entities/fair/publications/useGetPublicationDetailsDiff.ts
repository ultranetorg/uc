import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetPublicationDetailsDiff = (publicationId?: string, version?: number) => {
  const queryFn = () => api.getPublicationDetailsDiff(publicationId!, version!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: publicationsKeys.diff(publicationId!, version!),
    queryFn: queryFn,
    enabled: !!publicationId && version !== undefined,
  })

  return { isLoading, isFetching, isError, data }
}
