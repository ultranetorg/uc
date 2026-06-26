import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetPublisherPublications = (siteId?: string, authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getPublisherPublications(siteId!, authorId!, page, pageSize)

  const { isPending, isError, data } = useQuery({
    queryKey: publicationsKeys.authorPublications(siteId!, authorId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!siteId && !!authorId,
  })

  return { isPending, isError, data }
}
