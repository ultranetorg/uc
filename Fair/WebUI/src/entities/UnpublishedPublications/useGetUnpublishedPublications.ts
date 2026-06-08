import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { unpublishedPublicationsKeys } from "./unpublishedPublicationsKeys"

const api = getFairApi()

export const useGetUnpublishedPublications = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUnpublishedPublications(siteId!, page, pageSize)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: unpublishedPublicationsKeys.paged(siteId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isLoading, isFetching, isError, data }
}
