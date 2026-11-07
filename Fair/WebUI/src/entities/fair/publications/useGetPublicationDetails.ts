import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetPublicationDetails = (publicationId?: string) => {
  const queryFn = () => api.getPublicationDetails(publicationId!)

  const { isLoading, isError, data, error } = useQuery({
    queryKey: publicationsKeys.detail(publicationId!),
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isLoading, isError, data, error: error ?? undefined }
}
