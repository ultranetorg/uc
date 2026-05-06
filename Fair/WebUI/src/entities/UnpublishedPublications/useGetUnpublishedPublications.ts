import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { unpublishedPublicationsKeys } from "./unpublishedPublicationsKeys"

const api = getApi()

export const useGetUnpublishedPublications = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUnpublishedPublications(siteId!, page, pageSize)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: unpublishedPublicationsKeys.paged(siteId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isLoading, isFetching, isError, data }
}
