import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { unpublishedPublicationsKeys } from "./unpublishedPublicationsKeys"

const api = getFairApi()

export const useGetUnpublishedPublication = (storeId?: string, publicationId?: string) => {
  const queryFn = () => api.getUnpublishedPublication(storeId!, publicationId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: unpublishedPublicationsKeys.detail(storeId!, publicationId!),
    queryFn: queryFn,
    enabled: !!storeId && !!publicationId,
  })

  return { isLoading, isFetching, isError, data }
}
