import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { unpublishedPublicationsKeys } from "./unpublishedPublicationsKeys"

const api = getFairApi()

export const useGetUnpublishedPublication = (siteId?: string, publicationId?: string) => {
  const queryFn = () => api.getUnpublishedPublication(siteId!, publicationId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: unpublishedPublicationsKeys.detail(siteId!, publicationId!),
    queryFn: queryFn,
    enabled: !!siteId && !!publicationId,
  })

  return { isLoading, isFetching, isError, data }
}
