import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { unpublishedPublicationsKeys } from "./unpublishedPublicationsKeys"

const api = getFairApi()

export const useGetUnpublishedPublications = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUnpublishedPublications(storeId!, page, pageSize)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: unpublishedPublicationsKeys.paged(storeId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isFetching, isError, data }
}
