import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { changedPublicationsKeys } from "./changedPublicationsKeys"

const api = getFairApi()

export const useGetChangedPublications = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getChangedPublications(storeId!, page, pageSize)

  const { isFetching, isError, data } = useQuery({
    queryKey: changedPublicationsKeys.paged(storeId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!storeId,
  })
  return { isFetching, isError, data }
}
