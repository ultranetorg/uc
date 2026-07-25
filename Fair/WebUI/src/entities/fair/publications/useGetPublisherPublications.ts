import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetPublisherPublications = (storeId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getPublisherPublications(storeId!, authorId!, page, pageSize)

  const { isPending, isError, data } = useQuery({
    queryKey: publicationsKeys.authorPublications(storeId!, authorId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!storeId && !!authorId,
  })

  return { isPending, isError, data }
}
