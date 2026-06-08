import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetPublicationVersions = (publicationId?: string) => {
  const queryFn = () => api.getPublicationVersions(publicationId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: publicationsKeys.lastVersion(publicationId!),
    queryFn: queryFn,
    enabled: !!publicationId,
  })
  return { isFetching, isError, data }
}
