import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetPublisherPublications = (storeId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getPublisherPublications(storeId!, authorId!, page, pageSize)

  const { isLoading, isError, data } = useQuery({
    queryKey: publicationsKeys.authorPublications(storeId!, authorId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!storeId && !!authorId,
  })

  return { isLoading, isError, data }
}
